using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    // TODO : refactor first then cover with tests
    // FIXME : fix Source for new values. maybe pass from outside
    [ExcludeFromCodeCoverage]
    public static class SqlBigIntTypeConverter
    {
        public static SqlBigIntTypeValue ConvertValueFrom(this SqlBigIntTypeHandler typeHandler, SqlValue src, string targetType)
        {
            if (src is null)
            {
                return default;
            }

            if (src is SqlBigIntTypeValue intSrc
            && string.Equals(intSrc.TypeName, targetType, StringComparison.OrdinalIgnoreCase))
            {
                return intSrc;
            }

            var targetTypeRef = typeHandler.BigIntValueFactory.MakeSqlTypeReference(targetType);

            return typeHandler.DoConvertValueFrom(src, targetTypeRef, false, false);
        }

        public static SqlBigIntTypeValue ConvertValueFrom(this SqlBigIntTypeHandler typeHandler, SqlValue src, SqlBigIntTypeReference targetType, bool forceTargetType)
        {
            return typeHandler.DoConvertValueFrom(src, targetType, true, forceTargetType);
        }

        // TODO : implement strictTypeSize and forceTargetType support or get rid of them
        private static SqlBigIntTypeValue DoConvertValueFrom(
            this SqlBigIntTypeHandler typeHandler,
            SqlValue src,
            SqlBigIntTypeReference targetType,
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
                return typeHandler.BigIntValueFactory.MakeNullValue(targetType.TypeName, src.Source);
            }

            // converting from string
            if (src is SqlStrTypeValue strSrc)
            {
                if (!strSrc.IsPreciseValue)
                {
                    return typeHandler.BigIntValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                if (!BigInteger.TryParse(strSrc.Value, out BigInteger intValue))
                {
                    typeHandler.Violations.RegisterViolation(new UnableToConvertViolation(strSrc.Value, targetType.TypeName, src.Source));
                    return typeHandler.BigIntValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                /*
                if (intValue < targetType.Size.Low || intValue > targetType.Size.High)
                {
                    typeHandler.Violations.RegisterViolation(new OutOfRangeViolation(targetType.TypeName, intValue, src.Source));
                    // TODO : or nulls?
                    return typeHandler.IntValueFactory.MakeUnknownValue(targetType.TypeName);
                }
                */

                return typeHandler.BigIntValueFactory.MakePreciseValue(targetType.TypeName, intValue, src.Source);
            }

            // converting from int of different size
            if (src is SqlIntTypeValue intSrc)
            {
                if (!src.IsPreciseValue)
                {
                    return typeHandler.BigIntValueFactory.MakeApproximateValue(
                        targetType.TypeName,
                        new SqlBigIntValueRange(intSrc.EstimatedSize.Low, intSrc.EstimatedSize.High),
                        intSrc.Source);
                }

                BigInteger intValue = intSrc.Value;
                /*
                if (intValue < targetType.Size.Low || intValue > targetType.Size.High)
                {
                    typeHandler.Violations.RegisterViolation(new OutOfRangeViolation(targetType.TypeName, intValue, src.Source));

                    // TODO : or null?
                    return typeHandler.IntValueFactory.MakeUnknownValue(targetType.TypeName);
                }
                */

                return typeHandler.BigIntValueFactory.MakePreciseValue(targetType.TypeName, intValue, src.Source);
            }

            // converting from int of different size
            if (src is SqlBigIntTypeValue bigintSrc)
            {
                if (!src.IsPreciseValue)
                {
                    return typeHandler.BigIntValueFactory.MakeApproximateValue(targetType.TypeName, bigintSrc.EstimatedSize, bigintSrc.Source);
                }

                BigInteger intValue = bigintSrc.Value;
                /*
                if (intValue < targetType.Size.Low || intValue > targetType.Size.High)
                {
                    typeHandler.Violations.RegisterViolation(new OutOfRangeViolation(targetType.TypeName, intValue, src.Source));

                    // TODO : or null?
                    return typeHandler.IntValueFactory.MakeUnknownValue(targetType.TypeName);
                }
                */

                return typeHandler.BigIntValueFactory.MakePreciseValue(targetType.TypeName, intValue, src.Source);
            }

            // TODO : register violation about impossible conversion?
            return default;
        }
    }
}
