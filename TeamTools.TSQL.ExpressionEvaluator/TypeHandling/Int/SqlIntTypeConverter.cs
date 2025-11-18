using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // TODO : refactor first then cover with tests
    // FIXME : fix Source for new values. maybe pass from outside
    [ExcludeFromCodeCoverage]
    public static class SqlIntTypeConverter
    {
        public static SqlIntTypeValue ConvertValueFrom(this SqlIntTypeHandler typeHandler, SqlValue src, string targetType)
        {
            if (src is null)
            {
                return default;
            }

            if (src is SqlIntTypeValue intSrc
            && string.Equals(intSrc.TypeName, targetType, StringComparison.OrdinalIgnoreCase))
            {
                return intSrc;
            }

            var targetTypeRef = typeHandler.IntValueFactory.MakeSqlTypeReference(targetType);

            return typeHandler.ConvertValueFrom(src, targetTypeRef, false);
        }

        public static SqlIntTypeValue ConvertValueFrom(this SqlIntTypeHandler typeHandler, SqlValue src, SqlIntTypeReference targetType, bool forceTargetType)
        {
            return typeHandler.DoConvertValueFrom(src, targetType, true, forceTargetType);
        }

        // TODO : implement strictTypeSize and forceTargetType support or get rid of them
        private static SqlIntTypeValue DoConvertValueFrom(
            this SqlIntTypeHandler typeHandler,
            SqlValue src,
            SqlIntTypeReference targetType,
            bool strictTypeSize,
            bool forceTargetType)
        {
            if (src is null)
            {
                return default;
            }

            if (src.IsNull)
            {
                // TODO : not sure about passing src source through
                return typeHandler.IntValueFactory.MakeNullValue(targetType.TypeName, src.Source);
            }

            // converting from string
            if (src is SqlStrTypeValue strSrc)
            {
                if (!strSrc.IsPreciseValue)
                {
                    return typeHandler.IntValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                if (!int.TryParse(strSrc.Value, out int intValue))
                {
                    // TODO : only minus or plus is fine too
                    if (string.IsNullOrEmpty(strSrc.Value))
                    {
                        // TODO : this is still a very strange choice
                        // to assign/convert empty string to an INT
                        // why not to use 0?
                        intValue = 0;
                    }
                    else
                    {
                        typeHandler.Violations.RegisterViolation(new UnableToConvertViolation("'" + strSrc.Value + "'", targetType.TypeName, src.Source));

                        return typeHandler.IntValueFactory.MakeUnknownValue(targetType.TypeName);
                    }
                }

                if (intValue < targetType.Size.Low || intValue > targetType.Size.High)
                {
                    typeHandler.Violations.RegisterViolation(new OutOfRangeViolation(targetType.TypeName, intValue.ToString(), src.Source));
                    // TODO : or nulls?
                    return typeHandler.IntValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.IntValueFactory.MakePreciseValue(targetType.TypeName, intValue, src.Source);
            }

            // converting from int of different size
            if (src is SqlIntTypeValue intSrc)
            {
                if (!intSrc.IsPreciseValue)
                {
                    var targetSize = intSrc.EstimatedSize;
                    if (targetType.Size.CompareTo(intSrc.EstimatedSize) < 0)
                    {
                        typeHandler.Violations.RegisterViolation(new OutOfRangeViolation(targetType.TypeName, string.Format("{0}-{1}", intSrc.EstimatedSize.Low, intSrc.EstimatedSize.High), src.Source));
                        targetSize = targetType.Size;
                    }

                    return typeHandler.IntValueFactory.MakeApproximateValue(targetType.TypeName, targetSize, intSrc.Source);
                }

                int intValue = intSrc.Value;
                if (intValue < targetType.Size.Low || intValue > targetType.Size.High)
                {
                    typeHandler.Violations.RegisterViolation(new OutOfRangeViolation(targetType.TypeName, intValue.ToString(), src.Source));

                    // TODO : or null?
                    return typeHandler.IntValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.IntValueFactory.MakePreciseValue(targetType.TypeName, intValue, src.Source);
            }

            // converting from int of different size
            if (src is SqlBigIntTypeValue bigintSrc)
            {
                if (bigintSrc.EstimatedSize.Low < targetType.Size.Low || bigintSrc.EstimatedSize.High > targetType.Size.High)
                {
                    // FIXME : implement not precice case correctly
                    typeHandler.Violations.RegisterViolation(new OutOfRangeViolation(targetType.TypeName, bigintSrc.Value.ToString(), src.Source));

                    // TODO : or null?
                    return typeHandler.IntValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                if (!bigintSrc.IsPreciseValue)
                {
                    return typeHandler.IntValueFactory.MakeApproximateValue(
                        targetType.TypeName,
                        targetType.Size,
                        bigintSrc.Source);
                }

                // we checked
                int intValue = (int)bigintSrc.Value;

                return typeHandler.IntValueFactory.MakePreciseValue(targetType.TypeName, intValue, src.Source);
            }

            // TODO : register violation about impossible conversion?
            return default;
        }
    }
}
