using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlStrTypeReference : SqlGenericTypeReference<int>
    {
        public SqlStrTypeReference(string typeName, int size, ISqlValueFactory valueFactory)
        : base(typeName, size, valueFactory)
        {
            // TODO : currently string with unknown length will be created as size = -1
            // not sure if this is a good idea
            /*
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, "String type max length must be positive");
            }
            */

            HasFixedLength = !typeName.StartsWith("VAR", StringComparison.OrdinalIgnoreCase)
                && !typeName.StartsWith("NVAR", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsUnicode => GetIsUnicode();

        public bool HasFixedLength { get; }

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
