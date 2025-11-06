using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlStrTypeReference : SqlGenericTypeReference<int>
    {
        private static readonly ICollection<string> SupportedTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        // TODO : put to one place
        static SqlStrTypeReference()
        {
            SupportedTypes.Add("dbo.CHAR");
            SupportedTypes.Add("dbo.NCHAR");
            SupportedTypes.Add("dbo.VARCHAR");
            SupportedTypes.Add("dbo.NVARCHAR");
            SupportedTypes.Add("dbo.SYSNAME");
        }

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
                && (other is SqlStrTypeReference strRef)
                && strRef.Size.Equals(Size);
        }

        public override string ToString()
        {
            string length = Size > 8000 ? "MAX" : Size.ToString();
            return $"{TypeName}({length})";
        }

        protected virtual bool GetIsUnicode() => false;

        protected override int GetBytes() => Size > 0 ? Size : 0;
    }
}
