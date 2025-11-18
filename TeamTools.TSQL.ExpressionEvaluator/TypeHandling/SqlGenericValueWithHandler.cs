using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public abstract class SqlGenericValueWithHandler<THandler, TValue, TSize, TPreciseValue, TClone> : SqlGenericValue<TSize, TPreciseValue>, IDeepClonableValue<TClone>, IClonableValue
        where THandler : SqlGenericTypeHandler<TValue, TSize, TPreciseValue>
        where TValue : SqlGenericValue<TSize, TPreciseValue>
        where TSize : IComparable<TSize>
        where TPreciseValue : IComparable<TPreciseValue>
        where TClone : SqlGenericValueWithHandler<THandler, TValue, TSize, TPreciseValue, TClone>
    {
        private readonly THandler typeHandler;

        protected SqlGenericValueWithHandler(THandler typeHandler, SqlGenericTypeReference<TSize> estimatedSize, SqlValueKind valueKind, SqlValueSource source)
        : base(estimatedSize, valueKind, source)
        {
            this.typeHandler = typeHandler;
        }

        protected SqlGenericValueWithHandler(THandler typeHandler, SqlGenericTypeReference<TSize> specificSize, TPreciseValue value, SqlValueSource source)
        : base(specificSize, value, source)
        {
            this.typeHandler = typeHandler;
        }

        // For cloning
        protected SqlGenericValueWithHandler(TClone src, TPreciseValue value)
        : this(src.TypeHandler, src.TypeReference as SqlGenericTypeReference<TSize>, value, src.Source?.Clone())
        {
        }

        // For cloning
        protected SqlGenericValueWithHandler(TClone src)
        : this(src.TypeHandler, src.TypeReference as SqlGenericTypeReference<TSize>, src.ValueKind, src.Source?.Clone())
        {
        }

        // TODO : move to SqlTypeReference?
        public THandler TypeHandler => typeHandler;

        public override ISqlTypeHandler GetTypeHandler() => TypeHandler;

        public SqlValue Clone() => DeepClone();

        public abstract TClone DeepClone();
    }
}
