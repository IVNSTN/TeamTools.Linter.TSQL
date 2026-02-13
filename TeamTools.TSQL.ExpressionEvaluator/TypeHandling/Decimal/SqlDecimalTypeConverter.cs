using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    [ExcludeFromCodeCoverage]
    public static class SqlDecimalTypeConverter
    {
        public static SqlDecimalTypeValue ConvertValueFrom(this SqlDecimalTypeHandler typeHandler, SqlValue src, string targetType)
        {
            if (src is null)
            {
                return default;
            }

            if (src is SqlDecimalTypeValue decSrc
            && string.Equals(decSrc.TypeName, targetType, StringComparison.OrdinalIgnoreCase))
            {
                return decSrc;
            }

            var targetTypeRef = typeHandler.DecimalValueFactory.MakeSqlDataTypeReference(targetType);

            return typeHandler.ConvertValueFrom(src, targetTypeRef, false);
        }

        public static SqlDecimalTypeValue ConvertValueFrom(this SqlDecimalTypeHandler typeHandler, SqlValue src, SqlDecimalTypeReference targetType, bool forceTargetType)
        {
            return typeHandler.DoConvertValueFrom(src, targetType, true, forceTargetType);
        }

        // TODO : implement strictTypeSize and forceTargetType support or get rid of them
        private static SqlDecimalTypeValue DoConvertValueFrom(
            this SqlDecimalTypeHandler typeHandler,
            SqlValue src,
            SqlDecimalTypeReference targetType,
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
                return typeHandler.DecimalValueFactory.MakeNullValue(targetType.TypeName, src.Source);
            }

            // converting from int
            if (src is SqlIntTypeValue intSrc)
            {
                if (!intSrc.IsPreciseValue)
                {
                    return typeHandler.DecimalValueFactory.MakeApproximateValue(targetType.TypeName, targetType.Size, src.Source);
                }

                // TODO : detect Scale loss
                // TODO : check precision and out of range error
                return typeHandler.DecimalValueFactory.MakePreciseValue(targetType.TypeName, intSrc.Value, src.Source);
            }

            // converting from bigint
            if (src is SqlBigIntTypeValue bigintSrc)
            {
                if (!bigintSrc.IsPreciseValue)
                {
                    return typeHandler.DecimalValueFactory.MakeApproximateValue(targetType.TypeName, targetType.Size, src.Source);
                }

                // TODO : detect Scale loss
                // TODO : check precision and out of range error
                return typeHandler.DecimalValueFactory.MakePreciseValue(targetType.TypeName, (decimal)bigintSrc.Value, src.Source);
            }

            // converting from another decimal
            if (src is SqlDecimalTypeValue decSrc)
            {
                if (!decSrc.IsPreciseValue)
                {
                    return typeHandler.DecimalValueFactory.MakeApproximateValue(targetType.TypeName, targetType.Size, src.Source);
                }

                // TODO : detect actual Scale loss
                decimal number = decSrc.Value;
                if (targetType.Size.Scale < decSrc.EstimatedSize.Scale)
                {
                    number = Math.Round(number, targetType.Size.Scale);
                }

                // TODO : check precision and out of range error
                return typeHandler.DecimalValueFactory.MakePreciseValue(targetType.TypeName, number, src.Source);
            }

            // converting from another string
            if (src is SqlStrTypeValue strSrc)
            {
                if (!strSrc.IsPreciseValue || !decimal.TryParse(strSrc.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decValue))
                {
                    return typeHandler.DecimalValueFactory.MakeApproximateValue(targetType.TypeName, targetType.Size, src.Source);
                }

                // TODO : check Precision and out of range error
                return typeHandler.DecimalValueFactory.MakePreciseValue(targetType.TypeName, decValue, src.Source);
            }

            return default;
        }
    }
}
