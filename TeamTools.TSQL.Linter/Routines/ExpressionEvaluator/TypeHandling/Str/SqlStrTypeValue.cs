namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling
{
    public class SqlStrTypeValue : SqlGenericValueWithHandler<SqlStrTypeHandler, SqlStrTypeValue, int, string, SqlStrTypeValue>
    {
        public SqlStrTypeValue(SqlStrTypeHandler typeHandler, SqlStrTypeReference typeReference, SqlValueKind valueKind, SqlValueSource source)
        : base(typeHandler, typeReference, valueKind, source)
        {
        }

        public SqlStrTypeValue(SqlStrTypeHandler typeHandler, SqlStrTypeReference typeReference, string value, SqlValueSource source)
        : base(typeHandler, typeReference, value, source)
        {
        }

        // For cloning
        protected SqlStrTypeValue(SqlStrTypeValue src, string value) : base(src, value)
        {
        }

        // For cloning
        protected SqlStrTypeValue(SqlStrTypeValue src) : base(src)
        {
        }

        public SqlStrTypeValue ChangeTo(string newValue, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newValue, source);

        public SqlStrTypeValue ChangeTo(int newSize, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newSize, source);

        public override SqlStrTypeValue DeepClone()
        {
            if (IsPreciseValue && !IsNull)
            {
                return new SqlStrTypeValue(this, Value);
            }

            return new SqlStrTypeValue(this);
        }
    }
}
