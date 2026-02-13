using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDecimalTypeValue : SqlGenericValueWithHandler<SqlDecimalTypeHandler, SqlDecimalTypeValue, SqlDecimalValueRange, decimal, SqlDecimalTypeValue>
    {
        public SqlDecimalTypeValue(SqlDecimalTypeHandler typeHandler, SqlDecimalTypeReference typeReference, SqlValueKind valueKind, SqlValueSource source)
        : base(typeHandler, typeReference, valueKind, source)
        {
        }

        public SqlDecimalTypeValue(SqlDecimalTypeHandler typeHandler, SqlDecimalTypeReference typeReference, decimal value, SqlValueSource source)
        : base(typeHandler, typeReference, value, source)
        {
        }

        // For cloning
        protected SqlDecimalTypeValue(SqlDecimalTypeValue src, decimal value) : base(src, value)
        {
        }

        // For cloning
        protected SqlDecimalTypeValue(SqlDecimalTypeValue src) : base(src)
        {
        }

        public SqlDecimalTypeValue ChangeTo(decimal newValue, SqlValueSource source)
            => TypeHandler.ChangeValueTo(this, newValue, source);

        public SqlDecimalTypeValue ChangeTo(SqlDecimalValueRange newSize, SqlValueSource source)
            => TypeHandler.ChangeValueTo(this, newSize, source);

        public override SqlDecimalTypeValue DeepClone()
        {
            if (IsPreciseValue && !IsNull)
            {
                return new SqlDecimalTypeValue(this, Value);
            }

            return new SqlDecimalTypeValue(this);
        }
    }
}
