using System;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public abstract class SqlGenericValue<TSize, TPreciseValue> : SqlValue
    where TSize : IComparable<TSize>
    where TPreciseValue : IComparable<TPreciseValue>
    {
        private readonly SqlGenericTypeReference<TSize> estimatedSize;
        private readonly TPreciseValue value;

        protected SqlGenericValue(SqlGenericTypeReference<TSize> estimatedSize, SqlValueKind valueKind, SqlValueSource source)
        : base(estimatedSize.TypeName, valueKind, source)
        {
            this.estimatedSize = estimatedSize;
            value = default;
        }

        protected SqlGenericValue(SqlGenericTypeReference<TSize> specificSize, TPreciseValue value, SqlValueSource source)
        : base(specificSize.TypeName, SqlValueKind.Precise, source)
        {
            estimatedSize = specificSize;
            this.value = value;
        }

        public TSize EstimatedSize => GetEstimatedSize();

        // TODO : or error if not IsPreciseValue?
        public TPreciseValue Value => IsPreciseValue ? value : default;

        public override SqlTypeReference GetTypeReference() => estimatedSize;

        private TSize GetEstimatedSize() => estimatedSize is null ? default : estimatedSize.Size;
    }
}
