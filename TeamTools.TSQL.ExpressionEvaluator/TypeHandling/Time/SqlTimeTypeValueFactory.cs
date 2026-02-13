using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public sealed class SqlTimeTypeValueFactory : SqlGenericDateTimeTypeValueFactory<TimeSpan, SqlTimeOnlyValue>
    {
        private static readonly HashSet<string> TimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TIME",
        };

        // TODO : it should be readonly
        public SqlTimeTypeHandler TypeHandler { get; internal set; }

        public override SqlTimeOnlyValue MakeApproximateValue(string typeName, SqlDateTimeValueRange estimatedSize, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlTimeOnlyValue(
                TypeHandler,
                new SqlDateTimeTypeReference(typeName, estimatedSize, this),
                SqlValueKind.Unknown,
                source);
        }

        public override SqlTimeOnlyValue MakePreciseValue(string typeName, TimeSpan value, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlTimeOnlyValue(
                TypeHandler,
                new SqlDateTimeTypeReference(typeName, new SqlDateTimeValueRange(value), this),
                value,
                source);
        }

        public override SqlTimeOnlyValue MakeNullValue(string typeName, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlTimeOnlyValue(
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

            if (!TimeSpan.TryParse(value, out var timeValue))
            {
                return DoMakeValue(
                    typeName,
                    SqlValueKind.Unknown,
                    new SqlValueSource(SqlValueSourceKind.Literal, source));
            }

            return MakeLiteral(typeName, timeValue, source);
        }

        public override SqlValue NewNull(TSqlFragment source) => MakeNull(source);

        public override SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind) => DoMakeValue(typeRef.TypeName, valueKind);

        public override SqlTimeOnlyValue MakeUnknownValue(string typeName) => DoMakeValue(typeName, SqlValueKind.Unknown);

        public SqlDateTimeTypeReference MakeSqlDataTypeReference(string typeName)
        {
            var typeSize = GetDefaultTypeSize(typeName);

            if (typeSize is null)
            {
                return default;
            }

            return new SqlDateTimeTypeReference(typeName, typeSize, this);
        }

        protected override ICollection<string> GetSupportedTypes() => TimeTypes;

        private SqlTimeOnlyValue DoMakeValue(string typeName, SqlValueKind valueKind, SqlValueSource source = null)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlTimeOnlyValue(
                TypeHandler,
                MakeSqlDataTypeReference(typeName),
                valueKind,
                source);
        }
    }
}
