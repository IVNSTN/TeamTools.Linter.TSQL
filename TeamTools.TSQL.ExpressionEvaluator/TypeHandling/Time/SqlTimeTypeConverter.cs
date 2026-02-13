using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // TODO : refactor first then cover with tests
    // FIXME : fix Source for new values. maybe pass from outside
    [ExcludeFromCodeCoverage]
    public static class SqlTimeTypeConverter
    {
        public static SqlTimeOnlyValue ConvertValueFrom(this SqlTimeTypeHandler typeHandler, SqlValue src, string targetType)
        {
            if (src is null)
            {
                return default;
            }

            if (src is SqlTimeOnlyValue timeSrc
            && string.Equals(timeSrc.TypeName, targetType, StringComparison.OrdinalIgnoreCase))
            {
                return timeSrc;
            }

            var targetTypeRef = typeHandler.TimeValueFactory.MakeSqlDataTypeReference(targetType);

            return typeHandler.ConvertValueFrom(src, targetTypeRef, false);
        }

        public static SqlTimeOnlyValue ConvertValueFrom(this SqlTimeTypeHandler typeHandler, SqlValue src, SqlDateTimeTypeReference targetType, bool forceTargetType)
        {
            return typeHandler.DoConvertValueFrom(src, targetType, true, forceTargetType);
        }

        private static SqlTimeOnlyValue DoConvertValueFrom(
            this SqlTimeTypeHandler typeHandler,
            SqlValue src,
            SqlDateTimeTypeReference targetType,
            bool strictTypeSize,
            bool forceTargetType)
        {
            if (src is null)
            {
                return default;
            }

            if (src.IsNull)
            {
                return typeHandler.TimeValueFactory.MakeNullValue(targetType.TypeName, src.Source);
            }

            // converting from string
            if (src is SqlStrTypeValue strSrc)
            {
                if (!strSrc.IsPreciseValue)
                {
                    return typeHandler.TimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return (SqlTimeOnlyValue)typeHandler.TimeValueFactory.NewLiteral(targetType.TypeName, strSrc.Value, src.Source.Node);
            }

            // converting from int
            if (src is SqlIntTypeValue intSrc)
            {
                if (!intSrc.IsPreciseValue)
                {
                    // TODO : use range if provided
                    return typeHandler.TimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.TimeValueFactory.MakePreciseValue(targetType.TypeName, new TimeSpan(intSrc.Value), src.Source);
            }

            // converting from bigint
            if (src is SqlBigIntTypeValue bigintSrc)
            {
                if (!bigintSrc.IsPreciseValue || bigintSrc.Value > int.MaxValue || bigintSrc.Value < int.MinValue)
                {
                    // TODO : use range if provided
                    return typeHandler.TimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.TimeValueFactory.MakePreciseValue(targetType.TypeName, new TimeSpan((int)bigintSrc.Value), src.Source);
            }

            // converting from date
            if (src is SqlDateOnlyValue dateSrc)
            {
                if (!dateSrc.IsPreciseValue)
                {
                    // TODO : use range if provided
                    return typeHandler.TimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.TimeValueFactory.MakePreciseValue(targetType.TypeName, TimeSpan.Zero, src.Source);
            }

            // converting from datetime
            if (src is SqlDateTimeValue datetimeSrc)
            {
                if (!datetimeSrc.IsPreciseValue)
                {
                    // TODO : use range if provided
                    return typeHandler.TimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.TimeValueFactory.MakePreciseValue(targetType.TypeName, datetimeSrc.Value.TimeOfDay, src.Source);
            }

            return default;
        }
    }
}
