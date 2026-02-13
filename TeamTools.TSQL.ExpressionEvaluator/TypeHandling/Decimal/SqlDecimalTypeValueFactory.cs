using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Globalization;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDecimalTypeValueFactory : SqlGenericValueFactory<SqlDecimalTypeValue, SqlDecimalValueRange, decimal>, ISqlValueFactory
    {
        private static readonly SqlDecimalValueRange DefaultValueRange = new SqlDecimalValueRange(decimal.MinValue, decimal.MaxValue, 18, 0);

        private static readonly HashSet<string> Types = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            TSqlDomainAttributes.Types.Decimal,
            "NUMERIC",
        };

        public SqlDecimalTypeValueFactory() : base(TSqlDomainAttributes.Types.Decimal)
        {
        }

        // TODO : it should be readonly
        public SqlDecimalTypeHandler TypeHandler { get; internal set; }

        public override SqlDecimalTypeValue MakePreciseValue(string typeName, decimal value, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            // Extracting precision and scale from provided value so the value can be represented in terms of SQL SERVER
            int scale = GetDecimalPlaces(value);
            int precision = GetPrecision(value, scale);

            return new SqlDecimalTypeValue(
                TypeHandler,
                new SqlDecimalTypeReference(typeName, new SqlDecimalValueRange(value, value, precision, scale), this),
                value,
                source);
        }

        public override SqlDecimalTypeValue MakeApproximateValue(string typeName, SqlDecimalValueRange estimatedSize, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDecimalTypeValue(
                TypeHandler,
                new SqlDecimalTypeReference(typeName, estimatedSize, this),
                SqlValueKind.Unknown,
                source);
        }

        public override SqlDecimalTypeValue MakeNullValue(string typeName, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDecimalTypeValue(
                TypeHandler,
                MakeSqlDataTypeReference(typeName),
                SqlValueKind.Null,
                source);
        }

        public override SqlDecimalTypeValue MakeUnknownValue(string typeName) => DoMakeValue(typeName, SqlValueKind.Unknown);

        // FIXME : low/high limit gets lost here if provided
        public SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind) => DoMakeValue(typeRef.TypeName, valueKind);

        public SqlValue NewNull(TSqlFragment source) => MakeNull(source);

        public SqlValue NewLiteral(string typeName, string value, TSqlFragment source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal decValue))
            {
                return DoMakeValue(
                    typeName,
                    SqlValueKind.Unknown,
                    new SqlValueSource(SqlValueSourceKind.Literal, source));
            }

            return MakeLiteral(typeName, decValue, source);
        }

        // DECIMAL(18,0) is the default option set
        public SqlDecimalTypeReference MakeSqlDataTypeReference(string typeName, int precision = 18, int scale = 0)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDecimalTypeReference(typeName, new SqlDecimalValueRange(decimal.MinValue, decimal.MaxValue, precision, scale), this);
        }

        public override SqlDecimalValueRange GetDefaultTypeSize(string typeName) => DefaultValueRange;

        // TODO : provide source
        protected SqlDecimalTypeValue DoMakeValue(string typeName, SqlValueKind valueKind, SqlValueSource source = null)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDecimalTypeValue(
                TypeHandler,
                MakeSqlDataTypeReference(typeName),
                valueKind,
                source);
        }

        protected override ICollection<string> GetSupportedTypes() => Types;

        /// <summary>
        /// Gets the number of digits after the decimal point in a decimal value.
        /// </summary>
        /// <param name="value">The decimal value.</param>
        /// <returns>The number of digits after the decimal point.</returns>
        private static int GetDecimalPlaces(decimal value)
        {
            // Get the internal representation of the decimal value.
            // The fourth element of the array contains the scale factor.
            int[] bits = decimal.GetBits(value);

            // The scale factor is stored in bits 16-23 of the fourth element.
            // We shift the bits to the right by 16 and then mask with 0xFF (255)
            // to get the scale factor.
            int scale = (bits[3] >> 16) & 0xFF;

            return scale;
        }

        private static int GetPrecision(decimal value, int scale)
        {
            // +1 for value < 10
            unchecked
            {
                return (int)Math.Floor(Math.Log10(Math.Abs((double)value))) + 1 + scale;
            }
        }
    }
}
