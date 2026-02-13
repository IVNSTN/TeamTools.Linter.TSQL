using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDateTimeTypeValueFactory : SqlGenericDateTimeTypeValueFactory<DateTime, SqlDateTimeValue>
    {
        private static readonly HashSet<string> DateTimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            TSqlDomainAttributes.Types.SmallDateTime,
            TSqlDomainAttributes.Types.DateTime,
            TSqlDomainAttributes.Types.DateTime2,
        };

        private static readonly string[] DateTimeFormats = new string[]
        {
            "yyyyMMdd'T'HH':'mm':'ss",
            "yyyyMMdd HH':'mm':'ss",
            "yyyyMMdd HH':'mm",
            "yyyyMMdd",
            "yyyy-MM-dd'T'HH':'mm':'ss",
            "yyyy-MM-dd HH':'mm':'ss",
            "yyyy-MM-dd HH':'mm",
            "yyyy-MM-dd",
        };

        // TODO : or always en-us?
        private readonly CultureInfo defaultCulture = Thread.CurrentThread.CurrentCulture;

        // TODO : it should be readonly
        public SqlDateTimeTypeHandler TypeHandler { get; internal set; }

        public override SqlDateTimeValue MakeApproximateValue(string typeName, SqlDateTimeValueRange estimatedSize, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDateTimeValue(
                TypeHandler,
                new SqlDateTimeTypeReference(typeName, estimatedSize, this),
                SqlValueKind.Unknown,
                source);
        }

        public override SqlDateTimeValue MakePreciseValue(string typeName, DateTime value, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDateTimeValue(
                TypeHandler,
                new SqlDateTimeTypeReference(typeName, new SqlDateTimeValueRange(value), this),
                value,
                source);
        }

        public override SqlDateTimeValue MakeNullValue(string typeName, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDateTimeValue(
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

            if (!DateTime.TryParseExact(value, DateTimeFormats, defaultCulture, DateTimeStyles.None, out var datetimeValue))
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

        public override SqlDateTimeValue MakeUnknownValue(string typeName) => DoMakeValue(typeName, SqlValueKind.Unknown);

        public SqlDateTimeTypeReference MakeSqlDataTypeReference(string typeName)
        {
            var typeSize = GetDefaultTypeSize(typeName);

            if (typeSize is null)
            {
                return default;
            }

            return new SqlDateTimeTypeReference(typeName, typeSize, this);
        }

        protected override ICollection<string> GetSupportedTypes() => DateTimeTypes;

        private SqlDateTimeValue DoMakeValue(string typeName, SqlValueKind valueKind, SqlValueSource source = null)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlDateTimeValue(
                TypeHandler,
                MakeSqlDataTypeReference(typeName),
                valueKind,
                source);
        }
    }
}
