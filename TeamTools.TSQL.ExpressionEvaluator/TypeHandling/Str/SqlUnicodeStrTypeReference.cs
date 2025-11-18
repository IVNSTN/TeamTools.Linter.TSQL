using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlUnicodeStrTypeReference : SqlStrTypeReference
    {
        private static readonly HashSet<string> SupportedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // TODO : put to one place
        static SqlUnicodeStrTypeReference()
        {
            SupportedTypes.Add(TSqlDomainAttributes.Types.NChar);
            SupportedTypes.Add(TSqlDomainAttributes.Types.NVarchar);
            SupportedTypes.Add(TSqlDomainAttributes.Types.SysName);
        }

        public SqlUnicodeStrTypeReference(string typeName, int size, ISqlValueFactory valueFactory)
        : base(typeName, size, valueFactory)
        {
            if (!SupportedTypes.Contains(typeName))
            {
                throw new ArgumentOutOfRangeException(nameof(typeName), "this type is not unicode");
            }
        }

        protected override bool GetIsUnicode() => true;

        protected override int GetBytes() => Size > 0 ? Size * 2 : 0;
    }
}
