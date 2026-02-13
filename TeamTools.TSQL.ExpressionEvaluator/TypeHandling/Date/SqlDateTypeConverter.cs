using System;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // TODO : refactor first then cover with tests
    // FIXME : fix Source for new values. maybe pass from outside
    [ExcludeFromCodeCoverage]
    public static class SqlDateTypeConverter
    {
        public static SqlDateOnlyValue ConvertValueFrom(this SqlDateTypeHandler typeHandler, SqlValue src, string targetType)
        {
            if (src is null)
            {
                return default;
            }

            if (src is SqlDateOnlyValue dateSrc
            && string.Equals(dateSrc.TypeName, targetType, StringComparison.OrdinalIgnoreCase))
            {
                return dateSrc;
            }

            var targetTypeRef = typeHandler.DateValueFactory.MakeSqlDataTypeReference(targetType);

            return typeHandler.ConvertValueFrom(src, targetTypeRef, false);
        }

        public static SqlDateOnlyValue ConvertValueFrom(this SqlDateTypeHandler typeHandler, SqlValue src, SqlDateTimeTypeReference targetType, bool forceTargetType)
        {
            return typeHandler.DoConvertValueFrom(src, targetType, true, forceTargetType);
        }

        private static SqlDateOnlyValue DoConvertValueFrom(
            this SqlDateTypeHandler typeHandler,
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
                return typeHandler.DateValueFactory.MakeNullValue(targetType.TypeName, src.Source);
            }

            // converting from string
            if (src is SqlStrTypeValue strSrc)
            {
                if (!strSrc.IsPreciseValue)
                {
                    return typeHandler.DateValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return (SqlDateOnlyValue)typeHandler.DateValueFactory.NewLiteral(targetType.TypeName, strSrc.Value, src.Source.Node);
            }

            // converting from int
            if (src is SqlIntTypeValue intSrc)
            {
                if (!intSrc.IsPreciseValue)
                {
                    // TODO : use range if provided
                    return typeHandler.DateValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.DateValueFactory.MakePreciseValue(targetType.TypeName, SqlDateTimeTypeReference.MinSqlValue.AddDays(intSrc.Value).Date, src.Source);
            }

            // converting from bigint
            if (src is SqlBigIntTypeValue bigintSrc)
            {
                if (!bigintSrc.IsPreciseValue || bigintSrc.Value > int.MaxValue || bigintSrc.Value < int.MinValue)
                {
                    // TODO : use range if provided
                    return typeHandler.DateValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.DateValueFactory.MakePreciseValue(targetType.TypeName, SqlDateTimeTypeReference.MinSqlValue.AddDays((int)bigintSrc.Value).Date, src.Source);
            }

            // converting from datetime
            if (src is SqlDateTimeValue datetimeSrc)
            {
                if (!datetimeSrc.IsPreciseValue)
                {
                    // TODO : use range if provided
                    return typeHandler.DateValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.DateValueFactory.MakePreciseValue(targetType.TypeName, datetimeSrc.Value.Date, src.Source);
            }

            // converting from time
            if (src is SqlTimeOnlyValue timeSrc)
            {
                if (!timeSrc.IsPreciseValue)
                {
                    // TODO : use range if provided
                    return typeHandler.DateValueFactory.MakeUnknownValue(targetType.TypeName);
                }

                return typeHandler.DateValueFactory.MakePreciseValue(targetType.TypeName, new DateTime(timeSrc.Value.Ticks).Date, src.Source);
            }

            return default;
        }
    }
}
