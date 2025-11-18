using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlStrTypeReference : SqlGenericTypeReference<int>
    {
        // TODO : put to one place
        private static readonly HashSet<string> SupportedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            TSqlDomainAttributes.Types.Char,
            TSqlDomainAttributes.Types.NChar,
            TSqlDomainAttributes.Types.Varchar,
            TSqlDomainAttributes.Types.NVarchar,
            TSqlDomainAttributes.Types.SysName,
        };

        public SqlStrTypeReference(string typeName, int size, ISqlValueFactory valueFactory)
        : base(typeName, size, valueFactory)
        {
            if (!SupportedTypes.Contains(typeName))
            {
                throw new ArgumentOutOfRangeException(nameof(typeName));
            }

            // TODO : currently string with unknown length will be created as size = -1
            // not sure if this is a good idea
            /*
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, "String type max length must be positive");
            }
            */
        }

        public bool IsUnicode => GetIsUnicode();

        public override bool Equals(SqlTypeReference other)
        {
            return base.Equals(other)
                && other is SqlStrTypeReference strRef
                && strRef.Size.Equals(Size);
        }

        public override string ToString()
        {
            const string MaxLiteral = "MAX";
            string length = Size > 8000 ? MaxLiteral : Size.ToString();
            return $"{TypeName}({length})";
        }

        protected virtual bool GetIsUnicode() => false;

        protected override int GetBytes() => Size > 0 ? Size : 0;
    }
}
