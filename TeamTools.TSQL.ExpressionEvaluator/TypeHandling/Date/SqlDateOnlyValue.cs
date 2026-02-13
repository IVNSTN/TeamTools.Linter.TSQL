using System;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // TODO : Switch to System.DateOnly after dropping netstandard2.0 support
    public class SqlDateOnlyValue : SqlGenericValueWithHandler<SqlDateTypeHandler, SqlDateOnlyValue, SqlDateTimeValueRange, DateTime, SqlDateOnlyValue>
    {
        public SqlDateOnlyValue(SqlDateTypeHandler typeHandler, SqlDateTimeTypeReference typeReference, SqlValueKind valueKind, SqlValueSource source)
        : base(typeHandler, typeReference, valueKind, source)
        {
        }

        public SqlDateOnlyValue(SqlDateTypeHandler typeHandler, SqlDateTimeTypeReference typeReference, DateTime value, SqlValueSource source)
        : base(typeHandler, typeReference, value, source)
        {
        }

        // For cloning
        protected SqlDateOnlyValue(SqlDateOnlyValue src, DateTime value) : base(src, value)
        {
        }

        // For cloning
        protected SqlDateOnlyValue(SqlDateOnlyValue src) : base(src)
        {
        }

        public override SqlDateOnlyValue DeepClone()
        {
            if (IsPreciseValue && !IsNull)
            {
                return new SqlDateOnlyValue(this, Value);
            }

            return new SqlDateOnlyValue(this);
        }

        public SqlDateOnlyValue ChangeTo(DateTime newValue, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newValue, source);

        public SqlDateOnlyValue ChangeTo(SqlDateTimeValueRange newSize, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newSize, source);
    }
}
