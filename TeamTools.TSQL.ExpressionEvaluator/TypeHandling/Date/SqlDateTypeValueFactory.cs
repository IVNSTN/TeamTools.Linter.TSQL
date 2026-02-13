using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Globalization;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public sealed class SqlDateTypeValueFactory : SqlGenericDateTimeTypeValueFactory<DateTime, SqlDateOnlyValue>
    {
        private static readonly HashSet<string> DateTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DATE",
        };

        private static readonly DateTimeFormatInfo DateTimeSqlAnsiFormat;

        static SqlDateTypeValueFactory()
        {
            DateTimeSqlAnsiFormat = (DateTimeFormatInfo)CultureInfo.GetCultureInfo("en-us").DateTimeFormat.Clone();
            DateTimeSqlAnsiFormat.FullDateTimePattern = "yyyyMMdd'T'HH':'mm':'ss";
            DateTimeSqlAnsiFormat.LongDatePattern = "yyyy'-'MM'-'dd";
            DateTimeSqlAnsiFormat.ShortDatePattern = "yyyyMMdd";
            DateTimeSqlAnsiFormat.ShortTimePattern = "HH':'mm";
            DateTimeSqlAnsiFormat.LongTimePattern = "HH':'mm':'ss";
        }

        // TODO : it should be readonly
        public SqlDateTypeHandler TypeHandler { get; internal set; }

        public override SqlDateOnlyValue MakeApproximateValue(string typeName, SqlDateTimeValueRange estimatedSize, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDateOnlyValue(
                TypeHandler,
                new SqlDateTimeTypeReference(typeName, estimatedSize, this),
                SqlValueKind.Unknown,
                source);
        }

        public override SqlDateOnlyValue MakePreciseValue(string typeName, DateTime value, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDateOnlyValue(
                TypeHandler,
                new SqlDateTimeTypeReference(typeName, new SqlDateTimeValueRange(value), this),
                value,
                source);
        }

        public override SqlDateOnlyValue MakeNullValue(string typeName, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDateOnlyValue(
                TypeHandler,
                MakeSqlDataTypeReference(typeName),
                SqlValueKind.Null,
                source);
        }

        public override SqlValue NewLiteral(string typeName, string value, TSqlFragment source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            if (!DateTime.TryParse(value, DateTimeSqlAnsiFormat, DateTimeStyles.None, out var datetimeValue)
            && !DateTime.TryParse(value, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.None, out datetimeValue))
            {
                return DoMakeValue(
                    typeName,
                    SqlValueKind.Unknown,
                    new SqlValueSource(SqlValueSourceKind.Literal, source));
            }

            return MakeLiteral(typeName, datetimeValue, source);
        }

        public override SqlValue NewNull(TSqlFragment source) => MakeNull(source);

        public override SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind) => DoMakeValue(typeRef.TypeName, valueKind);

        public override SqlDateOnlyValue MakeUnknownValue(string typeName) => DoMakeValue(typeName, SqlValueKind.Unknown);

        public SqlDateTimeTypeReference MakeSqlDataTypeReference(string typeName)
        {
            var typeSize = GetDefaultTypeSize(typeName);

            if (typeSize is null)
            {
                return default;
            }

            return new SqlDateTimeTypeReference(typeName, typeSize, this);
        }

        protected override ICollection<string> GetSupportedTypes() => DateTypes;

        private SqlDateOnlyValue DoMakeValue(string typeName, SqlValueKind valueKind, SqlValueSource source = null)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDateOnlyValue(
                TypeHandler,
                MakeSqlDataTypeReference(typeName),
                valueKind,
                source);
        }
    }
}
