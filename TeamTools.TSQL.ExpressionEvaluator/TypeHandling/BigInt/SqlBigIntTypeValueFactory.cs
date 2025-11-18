using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Numerics;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlBigIntTypeValueFactory : SqlGenericValueFactory<SqlBigIntTypeValue, SqlBigIntValueRange, BigInteger>, ISqlValueFactory
    {
        private static readonly string IntFallbackTypeName = TSqlDomainAttributes.Types.BigInt;
        private static readonly Dictionary<string, SqlBigIntValueRange> TypeRanges = new Dictionary<string, SqlBigIntValueRange>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Types;

        static SqlBigIntTypeValueFactory()
        {
            // TODO : fix range
            TypeRanges.Add(TSqlDomainAttributes.Types.BigInt, new SqlBigIntValueRange(int.MinValue, int.MaxValue));

            Types = new HashSet<string>(TypeRanges.Keys, StringComparer.OrdinalIgnoreCase);
        }

        public SqlBigIntTypeValueFactory() : base(IntFallbackTypeName)
        {
        }

        // TODO : it should be readonly
        public SqlBigIntTypeHandler TypeHandler { get; internal set; }

        public override SqlBigIntTypeValue MakePreciseValue(string typeName, BigInteger value, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlBigIntTypeValue(
                TypeHandler,
                new SqlBigIntTypeReference(typeName, new SqlBigIntValueRange(value, value), this),
                value,
                source);
        }

        public override SqlBigIntTypeValue MakeApproximateValue(string typeName, SqlBigIntValueRange estimatedSize, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlBigIntTypeValue(
                TypeHandler,
                new SqlBigIntTypeReference(typeName, estimatedSize, this),
                SqlValueKind.Unknown,
                source);
        }

        public override SqlBigIntTypeValue MakeNullValue(string typeName, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlBigIntTypeValue(
                TypeHandler,
                MakeSqlTypeReference(typeName),
                SqlValueKind.Null,
                source);
        }

        public override SqlBigIntTypeValue MakeUnknownValue(string typeName) => DoMakeValue(typeName, SqlValueKind.Unknown);

        // FIXME : low/high limit gets lost here if provided
        public SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind) => DoMakeValue(typeRef.TypeName, valueKind);

        public SqlValue NewNull(TSqlFragment source) => MakeNull(source);

        public SqlValue NewLiteral(string typeName, string value, TSqlFragment source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            if (!BigInteger.TryParse(value, out BigInteger intValue))
            {
                return new SqlBigIntTypeValue(
                    TypeHandler,
                    MakeSqlTypeReference(typeName),
                    SqlValueKind.Unknown,
                    new SqlValueSource(SqlValueSourceKind.Literal, source));
            }

            return MakeLiteral(typeName, intValue, source);
        }

        public SqlBigIntTypeReference MakeSqlTypeReference(string typeName)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlBigIntTypeReference(typeName, TypeRanges[typeName], this);
        }

        public override SqlBigIntValueRange GetDefaultTypeSize(string typeName)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return TypeRanges[typeName];
        }

        protected SqlBigIntTypeValue DoMakeValue(string typeName, SqlValueKind valueKind)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            // TODO : provide source
            return new SqlBigIntTypeValue(
                TypeHandler,
                MakeSqlTypeReference(typeName),
                valueKind,
                null);
        }

        protected override ICollection<string> GetSupportedTypes() => Types;
    }
}
