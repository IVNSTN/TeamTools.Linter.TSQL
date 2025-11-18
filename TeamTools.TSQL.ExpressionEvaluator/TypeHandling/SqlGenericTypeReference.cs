using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public abstract class SqlGenericTypeReference<TSize> : SqlTypeReference
    where TSize : IComparable<TSize>
    {
        protected SqlGenericTypeReference(string typeName, TSize size, ISqlValueFactory valueFactory)
        : base(typeName, valueFactory)
        {
            if (size == null)
            {
                throw new ArgumentNullException(nameof(size));
            }

            Size = size;
        }

        public TSize Size { get; }

        public int Bytes => GetBytes();

        public override int CompareTo(SqlTypeReference other)
            => other is SqlGenericTypeReference<TSize> typeWithSize
            && IsOfSameType(other)
            ? Size.CompareTo(typeWithSize.Size)
            // TODO : or error?
            : default;

        protected abstract int GetBytes();
    }
}
