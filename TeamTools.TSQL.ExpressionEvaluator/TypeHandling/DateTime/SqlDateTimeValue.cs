using System;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public class SqlDateTimeValue : SqlGenericValueWithHandler<SqlDateTimeTypeHandler, SqlDateTimeValue, SqlDateTimeValueRange, DateTime, SqlDateTimeValue>
    {
        public SqlDateTimeValue(SqlDateTimeTypeHandler typeHandler, SqlDateTimeTypeReference typeReference, SqlValueKind valueKind, SqlValueSource source)
        : base(typeHandler, typeReference, valueKind, source)
        {
        }

        public SqlDateTimeValue(SqlDateTimeTypeHandler typeHandler, SqlDateTimeTypeReference typeReference, DateTime value, SqlValueSource source)
        : base(typeHandler, typeReference, value, source)
        {
        }

        // For cloning
        protected SqlDateTimeValue(SqlDateTimeValue src, DateTime value) : base(src, value)
        {
        }

        // For cloning
        protected SqlDateTimeValue(SqlDateTimeValue src) : base(src)
        {
        }

        public override SqlDateTimeValue DeepClone()
        {
            if (IsPreciseValue && !IsNull)
            {
                return new SqlDateTimeValue(this, Value);
            }

            return new SqlDateTimeValue(this);
        }

        public SqlDateTimeValue ChangeTo(DateTime newValue, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newValue, source);

        public SqlDateTimeValue ChangeTo(SqlDateTimeValueRange newSize, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newSize, source);
    }
}
