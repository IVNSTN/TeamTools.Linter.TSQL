using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // TODO : refactor first then cover with tests
    // FIXME : fix Source for new values. maybe pass from outside
    [ExcludeFromCodeCoverage]
    public static class SqlDateTimeTypeConverter
    {
        public static SqlDateTimeValue ConvertValueFrom(this SqlDateTimeTypeHandler typeHandler, SqlValue src, string targetType)
        {
            if (src is null)
            {
                return default;
            }

            if (src is SqlDateTimeValue datetimeSrc
            && string.Equals(datetimeSrc.TypeName, targetType, StringComparison.OrdinalIgnoreCase))
            {
                return datetimeSrc;
            }

            var targetTypeRef = typeHandler.DateTimeValueFactory.MakeSqlDataTypeReference(targetType);

            return typeHandler.ConvertValueFrom(src, targetTypeRef, false);
        }

        public static SqlDateTimeValue ConvertValueFrom(this SqlDateTimeTypeHandler typeHandler, SqlValue src, SqlDateTimeTypeReference targetType, bool forceTargetType)
        {
            return typeHandler.DoConvertValueFrom(src, targetType, true, forceTargetType);
        }

        private static SqlDateTimeValue DoConvertValueFrom(
            this SqlDateTimeTypeHandler typeHandler,
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
                return typeHandler.DateTimeValueFactory.MakeNullValue(targetType.TypeName, src.Source);
            }

            // converting from string
            if (src is SqlStrTypeValue strSrc)
            {
                if (!strSrc.IsPreciseValue)
                {
                    return typeHandler.DateTimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return (SqlDateTimeValue)typeHandler.DateTimeValueFactory.NewLiteral(targetType.TypeName, strSrc.Value, src.Source.Node);
            }

            // converting from int
            if (src is SqlIntTypeValue intSrc)
            {
                if (!intSrc.IsPreciseValue)
                {
                    // TODO : use range if provided
                    return typeHandler.DateTimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.DateTimeValueFactory.MakePreciseValue(targetType.TypeName, SqlDateTimeTypeReference.MinSqlValue.AddDays(intSrc.Value).Date, src.Source);
            }

            // converting from bigint
            if (src is SqlBigIntTypeValue bigintSrc)
            {
                if (!bigintSrc.IsPreciseValue || bigintSrc.Value > int.MaxValue || bigintSrc.Value < int.MinValue)
                {
                    // TODO : use range if provided
                    return typeHandler.DateTimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.DateTimeValueFactory.MakePreciseValue(targetType.TypeName, SqlDateTimeTypeReference.MinSqlValue.AddDays((int)bigintSrc.Value).Date, src.Source);
            }

            // converting from date
            if (src is SqlDateOnlyValue dateSrc)
            {
                if (!dateSrc.IsPreciseValue)
                {
                    // TODO : use range if provided
                    return typeHandler.DateTimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.DateTimeValueFactory.MakePreciseValue(targetType.TypeName, dateSrc.Value, src.Source);
            }

            // converting from time
            if (src is SqlTimeOnlyValue timeSrc)
            {
                if (!timeSrc.IsPreciseValue)
                {
                    // TODO : use range if provided
                    return typeHandler.DateTimeValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.DateTimeValueFactory.MakePreciseValue(targetType.TypeName, new DateTime(timeSrc.Value.Ticks), src.Source);
            }

            return default;
        }
    }
}
