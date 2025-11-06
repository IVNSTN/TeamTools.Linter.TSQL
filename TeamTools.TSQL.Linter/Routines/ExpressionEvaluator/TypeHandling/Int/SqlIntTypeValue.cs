namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlIntTypeValue : SqlGenericValueWithHandler<SqlIntTypeHandler, SqlIntTypeValue, SqlIntValueRange, int, SqlIntTypeValue>
    {
        public SqlIntTypeValue(SqlIntTypeHandler typeHandler, SqlIntTypeReference typeReference, SqlValueKind valueKind, SqlValueSource source)
        : base(typeHandler, typeReference, valueKind, source)
        {
        }

        public SqlIntTypeValue(SqlIntTypeHandler typeHandler, SqlIntTypeReference typeReference, int value, SqlValueSource source)
        : base(typeHandler, typeReference, value, source)
        {
        }

        // For cloning
        protected SqlIntTypeValue(SqlIntTypeValue src, int value) : base(src, value)
        {
        }

        // For cloning
        protected SqlIntTypeValue(SqlIntTypeValue src) : base(src)
        {
        }

        public SqlIntTypeValue ChangeTo(int newValue, SqlValueSource source)
            => TypeHandler.ChangeValueTo(this, newValue, source);

        public SqlIntTypeValue ChangeTo(SqlIntValueRange newSize, SqlValueSource source)
            => TypeHandler.ChangeValueTo(this, newSize, source);

        public override SqlIntTypeValue DeepClone()
        {
            if (IsPreciseValue && !IsNull)
            {
                return new SqlIntTypeValue(this, Value);
            }

            return new SqlIntTypeValue(this);
        }
    }
}
