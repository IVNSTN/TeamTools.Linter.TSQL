using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlIntTypeValueFactory : SqlGenericValueFactory<SqlIntTypeValue, SqlIntValueRange, int>, ISqlValueFactory
    {
        private static readonly string IntFallbackTypeName = "dbo.INT";
        private static readonly IDictionary<string, SqlIntValueRange> TypeRanges = new Dictionary<string, SqlIntValueRange>(StringComparer.OrdinalIgnoreCase);

        static SqlIntTypeValueFactory()
        {
            TypeRanges.Add("dbo.BIT", new SqlIntValueRange(0, 1));
            TypeRanges.Add("dbo.TINYINT", new SqlIntValueRange(0, 255));
            TypeRanges.Add("dbo.SMALLINT", new SqlIntValueRange(-32768, 32767));
            TypeRanges.Add("dbo.INT", new SqlIntValueRange(int.MinValue, int.MaxValue));
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
                return new SqlIntTypeValue(
                    TypeHandler,
                    MakeSqlTypeReference(typeName),
                    SqlValueKind.Unknown,
                    new SqlValueSource(SqlValueSourceKind.Literal, source));
            }

            return MakeLiteral(typeName, intValue, source);
        }

        public SqlIntTypeReference MakeSqlTypeReference(string typeName)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlIntTypeReference(typeName, TypeRanges[typeName], this);
        }

        public override SqlIntValueRange GetDefaultTypeSize(string typeName)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return TypeRanges[typeName];
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

        protected override ICollection<string> GetSupportedTypes() => TypeRanges.Keys;
    }
}
