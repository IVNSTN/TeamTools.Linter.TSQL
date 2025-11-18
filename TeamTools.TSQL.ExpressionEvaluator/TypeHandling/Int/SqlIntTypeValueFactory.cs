using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlIntTypeValueFactory : SqlGenericValueFactory<SqlIntTypeValue, SqlIntValueRange, int>, ISqlValueFactory
    {
        private static readonly string IntFallbackTypeName = TSqlDomainAttributes.Types.Int;
        private static readonly Dictionary<string, SqlIntValueRange> TypeRanges = new Dictionary<string, SqlIntValueRange>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Types;

        static SqlIntTypeValueFactory()
        {
            TypeRanges.Add(TSqlDomainAttributes.Types.Bit, new SqlIntValueRange(0, 1));
            TypeRanges.Add(TSqlDomainAttributes.Types.TinyInt, new SqlIntValueRange(0, 255));
            TypeRanges.Add(TSqlDomainAttributes.Types.SmallInt, new SqlIntValueRange(-32768, 32767));
            TypeRanges.Add(TSqlDomainAttributes.Types.Int, new SqlIntValueRange(int.MinValue, int.MaxValue));

            Types = new HashSet<string>(TypeRanges.Keys, StringComparer.OrdinalIgnoreCase);
        }

        public SqlIntTypeValueFactory() : base(IntFallbackTypeName)
        {
        }

        // TODO : it should be readonly
        public SqlIntTypeHandler TypeHandler { get; internal set; }

        public override SqlIntTypeValue MakePreciseValue(string typeName, int value, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlIntTypeValue(
                TypeHandler,
                new SqlIntTypeReference(typeName, new SqlIntValueRange(value, value), this),
                value,
                source);
        }

        public override SqlIntTypeValue MakeApproximateValue(string typeName, SqlIntValueRange estimatedSize, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlIntTypeValue(
                TypeHandler,
                new SqlIntTypeReference(typeName, estimatedSize, this),
                SqlValueKind.Unknown,
                source);
        }

        public override SqlIntTypeValue MakeNullValue(string typeName, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlIntTypeValue(
                TypeHandler,
                MakeSqlTypeReference(typeName),
                SqlValueKind.Null,
                source);
        }

        public override SqlIntTypeValue MakeUnknownValue(string typeName) => DoMakeValue(typeName, SqlValueKind.Unknown);

        // FIXME : low/high limit gets lost here if provided
        public SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind) => DoMakeValue(typeRef.TypeName, valueKind);

        public SqlValue NewNull(TSqlFragment source) => MakeNull(source);

        public SqlValue NewLiteral(string typeName, string value, TSqlFragment source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            if (!int.TryParse(value, out int intValue))
            {
                return DoMakeValue(
                    typeName,
                    SqlValueKind.Unknown,
                    new SqlValueSource(SqlValueSourceKind.Literal, source));
            }

            return MakeLiteral(typeName, intValue, source);
        }

        public SqlIntTypeReference MakeSqlTypeReference(string typeName)
        {
            if (string.IsNullOrEmpty(typeName) || !TypeRanges.TryGetValue(typeName, out var typeSize))
            {
                return default;
            }

            return new SqlIntTypeReference(typeName, typeSize, this);
        }

        public override SqlIntValueRange GetDefaultTypeSize(string typeName)
        {
            if (string.IsNullOrEmpty(typeName) || !TypeRanges.TryGetValue(typeName, out var typeSize))
            {
                return default;
            }

            return typeSize;
        }

        // TODO : provide source
        protected SqlIntTypeValue DoMakeValue(string typeName, SqlValueKind valueKind, SqlValueSource source = null)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlIntTypeValue(
                TypeHandler,
                MakeSqlTypeReference(typeName),
                valueKind,
                source);
        }

        protected override ICollection<string> GetSupportedTypes() => Types;
    }
}
