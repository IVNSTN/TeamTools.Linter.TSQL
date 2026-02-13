using System;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    // TODO : Switch to System.TimeOnly after dropping netstandard2.0 support
    public class SqlTimeOnlyValue : SqlGenericValueWithHandler<SqlTimeTypeHandler, SqlTimeOnlyValue, SqlDateTimeValueRange, TimeSpan, SqlTimeOnlyValue>
    {
        public SqlTimeOnlyValue(SqlTimeTypeHandler typeHandler, SqlDateTimeTypeReference typeReference, SqlValueKind valueKind, SqlValueSource source)
        : base(typeHandler, typeReference, valueKind, source)
        {
        }

        public SqlTimeOnlyValue(SqlTimeTypeHandler typeHandler, SqlDateTimeTypeReference typeReference, TimeSpan value, SqlValueSource source)
        : base(typeHandler, typeReference, value, source)
        {
        }

        // For cloning
        protected SqlTimeOnlyValue(SqlTimeOnlyValue src, TimeSpan value) : base(src, value)
        {
        }

        // For cloning
        protected SqlTimeOnlyValue(SqlTimeOnlyValue src) : base(src)
        {
        }

        public override SqlTimeOnlyValue DeepClone()
        {
            if (IsPreciseValue && !IsNull)
            {
                return new SqlTimeOnlyValue(this, Value);
            }

            return new SqlTimeOnlyValue(this);
        }

        public SqlTimeOnlyValue ChangeTo(TimeSpan newValue, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newValue, source);

        public SqlTimeOnlyValue ChangeTo(SqlDateTimeValueRange newSize, SqlValueSource source) => TypeHandler.ChangeValueTo(this, newSize, source);
    }
}
