using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlBinaryTypeValueFactory : SqlGenericValueFactory<SqlBinaryTypeValue, int, HexValue>, ISqlValueFactory
    {
        private static readonly string BinaryFallbackTypeName = TSqlDomainAttributes.Types.Binary;
        private static readonly Dictionary<string, int> DefaultTypeSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Types;

        static SqlBinaryTypeValueFactory()
        {
            DefaultTypeSizes.Add(TSqlDomainAttributes.Types.Binary, 1);
            // FIXME : column or variable will be 1 symbol long
            // only CAST/CONVERT to VARBINARY without length results with 30-symbol long value
            DefaultTypeSizes.Add(TSqlDomainAttributes.Types.VarBinary, 30);

            Types = new HashSet<string>(DefaultTypeSizes.Keys, StringComparer.OrdinalIgnoreCase);
        }

        public SqlBinaryTypeValueFactory() : base(BinaryFallbackTypeName)
        {
        }

        // TODO : it should be readonly
        public SqlBinaryTypeHandler TypeHandler { get; internal set; }

        public override SqlBinaryTypeValue MakePreciseValue(string typeName, HexValue value, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                // TODO : or throw?
                return default;
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new SqlBinaryTypeValue(TypeHandler, new SqlBinaryTypeReference(typeName, value.AsString.Length / 2, this), value, source);
        }

        public override SqlBinaryTypeValue MakeApproximateValue(string typeName, int estimatedLength, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                // TODO : or throw?
                return default;
            }

            // TODO : value can be empty (LEN(@var) = 0)
            // but minimun string storage size is 1 char
            return new SqlBinaryTypeValue(
                TypeHandler,
                MakeSqlDataTypeReference(typeName, estimatedLength),
                SqlValueKind.Unknown,
                source);
        }

        public override SqlBinaryTypeValue MakeNullValue(string typeName, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                // TODO : or throw?
                return default;
            }

            return new SqlBinaryTypeValue(
                TypeHandler,
                MakeSqlDataTypeReference(typeName, GetDefaultTypeSize(typeName)),
                SqlValueKind.Null,
                source);
        }

        public override SqlBinaryTypeValue MakeUnknownValue(string typeName) => DoMakeValue(typeName, SqlValueKind.Unknown);

        public SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind)
        {
            // TODO : refactor this magic transition
            if (!(typeRef is SqlBinaryTypeReference binRef))
            {
                return default;
            }

            // TODO : pass source
            return new SqlBinaryTypeValue(
                TypeHandler,
                binRef,
                valueKind,
                null);
        }

        public SqlValue NewNull(TSqlFragment source) => MakeNull(source);

        public SqlValue NewLiteral(string typeName, string value, TSqlFragment source)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            if (string.IsNullOrEmpty(value))
            {
                return new SqlBinaryTypeValue(
                    TypeHandler,
                    MakeSqlDataTypeReference(typeName, GetDefaultTypeSize(typeName)),
                    SqlValueKind.Unknown,
                    new SqlValueSource(SqlValueSourceKind.Literal, source));
            }

            // TODO : HexValue.TryParse + create unknown if false?
            // value starts with 0x, division by 2 is truncated so this formula for fixed length in bytes is fine
            return MakeLiteral(typeName, new HexValue(value, (value.Length - 1) / 2), source);
        }

        public SqlBinaryTypeReference MakeSqlDataTypeReference(string typeName, int length)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            return new SqlBinaryTypeReference(typeName, length, this);
        }

        public override int GetDefaultTypeSize(string typeName)
        {
            DefaultTypeSizes.TryGetValue(typeName, out var typeSize);
            return typeSize;
        }

        // TODO : provide source
        protected SqlBinaryTypeValue DoMakeValue(string typeName, SqlValueKind valueKind, SqlValueSource source = null)
        {
            if (!IsTypeSupported(typeName))
            {
                // TODO : or throw?
                return default;
            }

            // FIXME : get rid of this -1 magic
            // if it is supposed to mean MAX - use static field instead of hardcoded value
            int size = -1;
            if (valueKind != SqlValueKind.Unknown)
            {
                size = GetDefaultTypeSize(typeName);
            }

            return new SqlBinaryTypeValue(
                TypeHandler,
                MakeSqlDataTypeReference(typeName, size),
                valueKind,
                source);
        }

        protected override ICollection<string> GetSupportedTypes() => Types;
    }
}
