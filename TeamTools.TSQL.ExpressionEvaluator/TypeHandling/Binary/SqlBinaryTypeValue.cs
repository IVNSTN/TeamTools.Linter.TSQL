using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlBinaryTypeValue : SqlGenericValueWithHandler<SqlBinaryTypeHandler, SqlBinaryTypeValue, int, HexValue, SqlBinaryTypeValue>
    {
        public SqlBinaryTypeValue(SqlBinaryTypeHandler typeHandler, SqlBinaryTypeReference typeReference, SqlValueKind valueKind, SqlValueSource source)
        : base(typeHandler, typeReference, valueKind, source)
        {
        }

        public SqlBinaryTypeValue(SqlBinaryTypeHandler typeHandler, SqlBinaryTypeReference typeReference, HexValue value, SqlValueSource source)
        : base(typeHandler, typeReference, value, source)
        {
        }

        // For cloning
        protected SqlBinaryTypeValue(SqlBinaryTypeValue src, HexValue value) : base(src, value)
        {
        }

        // For cloning
        protected SqlBinaryTypeValue(SqlBinaryTypeValue src) : base(src)
        {
        }

        // TODO : truncate if newValue is too long
        public SqlBinaryTypeValue ChangeTo(HexValue newValue, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newValue, source);

        public SqlBinaryTypeValue ChangeTo(int newSize, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newSize, source);

        public override SqlBinaryTypeValue DeepClone()
        {
            if (IsPreciseValue && !IsNull)
            {
                return new SqlBinaryTypeValue(this, new HexValue(Value.AsString, Value.MinBytes));
            }

            return new SqlBinaryTypeValue(this);
        }
    }
}
