using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlUnicodeStrTypeReference : SqlStrTypeReference
    {
        private static readonly ICollection<string> SupportedTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        // TODO : put to one place
        static SqlUnicodeStrTypeReference()
        {
            SupportedTypes.Add("dbo.NCHAR");
            SupportedTypes.Add("dbo.NVARCHAR");
            SupportedTypes.Add("dbo.SYSNAME");
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
