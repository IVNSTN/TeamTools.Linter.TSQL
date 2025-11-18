using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlStrTypeValueFactory : SqlGenericValueFactory<SqlStrTypeValue, int, string>, ISqlValueFactory
    {
        private static readonly string StrFallbackTypeName = TSqlDomainAttributes.Types.Varchar;
        private static readonly Dictionary<string, int> DefaultStringSizes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> UnicodeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Types;

        static SqlStrTypeValueFactory()
        {
            DefaultStringSizes.Add(TSqlDomainAttributes.Types.Char, 1);
            DefaultStringSizes.Add(TSqlDomainAttributes.Types.NChar, 1);
            DefaultStringSizes.Add(TSqlDomainAttributes.Types.Varchar, 30);
            DefaultStringSizes.Add(TSqlDomainAttributes.Types.NVarchar, 30);
            DefaultStringSizes.Add(TSqlDomainAttributes.Types.SysName, 128);

            UnicodeTypes.Add(TSqlDomainAttributes.Types.NChar);
            UnicodeTypes.Add(TSqlDomainAttributes.Types.NVarchar);
            UnicodeTypes.Add(TSqlDomainAttributes.Types.SysName);

            Types = new HashSet<string>(DefaultStringSizes.Keys, StringComparer.OrdinalIgnoreCase);
        }

        public SqlStrTypeValueFactory() : base(StrFallbackTypeName)
        {
        }

        // TODO : it should be readonly
        public SqlStrTypeHandler TypeHandler { get; internal set; }

        public override SqlStrTypeValue MakePreciseValue(string typeName, string value, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                // TODO : or throw?
                return default;
            }

            if (value is null)
            {
                value = "";
            }

            SqlStrTypeReference typeRef;

            if (IsUnicodeType(typeName))
            {
                typeRef = new SqlUnicodeStrTypeReference(typeName, value.Length, this);
            }
            else
            {
                /* TODO : or keep it here?
                if (!string.IsNullOrEmpty(value) && value.ContainsNationalCharacter())
                {
                    // Literal is not marked as N'' but contains unicode symbols - they will be lost
                    TypeHandler.Violations.RegisterViolation(new NationalSymbolFailureViolation("?", source));
                }
                */

                typeRef = new SqlStrTypeReference(typeName, value.Length, this);
            }

            return new SqlStrTypeValue(TypeHandler, typeRef, value, source);
        }

        public override SqlStrTypeValue MakeApproximateValue(string typeName, int estimatedLength, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                // TODO : or throw?
                return default;
            }

            // TODO : value can be empty (LEN(@var) = 0)
            // but minimun string storage size is 1 char
            return new SqlStrTypeValue(
                TypeHandler,
                MakeSqlTypeReference(typeName, estimatedLength),
                SqlValueKind.Unknown,
                source);
        }

        public override SqlStrTypeValue MakeNullValue(string typeName, SqlValueSource source)
        {
            if (!IsTypeSupported(typeName))
            {
                // TODO : or throw?
                return default;
            }

            return new SqlStrTypeValue(
                TypeHandler,
                MakeSqlTypeReference(typeName, GetDefaultTypeSize(typeName)),
                SqlValueKind.Null,
                source);
        }

        public override SqlStrTypeValue MakeUnknownValue(string typeName) => DoMakeValue(typeName, SqlValueKind.Unknown);

        public SqlValue NewValue(SqlTypeReference typeRef, SqlValueKind valueKind)
        {
            // TODO : refactor this magic transition
            if (!(typeRef is SqlStrTypeReference strRef))
            {
                return default;
            }

            // TODO : pass source
            return new SqlStrTypeValue(
                TypeHandler,
                strRef,
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

            if (value is null)
            {
                return new SqlStrTypeValue(
                    TypeHandler,
                    MakeSqlTypeReference(typeName, GetDefaultTypeSize(typeName)),
                    SqlValueKind.Unknown,
                    new SqlValueSource(SqlValueSourceKind.Literal, source));
            }

            return MakeLiteral(typeName, value, source);
        }

        public SqlStrTypeReference MakeSqlTypeReference(string typeName, int length)
        {
            if (!IsTypeSupported(typeName))
            {
                return default;
            }

            if (IsUnicodeType(typeName))
            {
                return new SqlUnicodeStrTypeReference(typeName, length, this);
            }

            return new SqlStrTypeReference(typeName, length, this);
        }

        public override int GetDefaultTypeSize(string typeName)
        {
            DefaultStringSizes.TryGetValue(typeName, out var typeSize);
            return typeSize;
        }

        // TODO : provide source
        protected SqlStrTypeValue DoMakeValue(string typeName, SqlValueKind valueKind, SqlValueSource source = null)
        {
            if (!IsTypeSupported(typeName))
            {
                // TODO : or throw?
                return default;
            }

            // FIXME : какая-то хрень с этой минус единицей
            // по коду поддержки почти нет
            // костыль про непонятное
            int size = -1;
            if (valueKind != SqlValueKind.Unknown)
            {
                size = GetDefaultTypeSize(typeName);
            }

            return new SqlStrTypeValue(
                TypeHandler,
                MakeSqlTypeReference(typeName, size),
                valueKind,
                source);
        }

        protected override ICollection<string> GetSupportedTypes() => Types;

        private static bool IsUnicodeType(string typeName) => UnicodeTypes.Contains(typeName);
    }
}
