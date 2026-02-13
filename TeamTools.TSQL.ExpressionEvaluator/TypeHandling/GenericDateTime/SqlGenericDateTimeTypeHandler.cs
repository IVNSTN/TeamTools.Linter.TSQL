using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public abstract class SqlGenericDateTimeTypeHandler<TValue, TSqlValue> : SqlGenericTypeHandler<TSqlValue, SqlDateTimeValueRange, TValue>
    where TValue : IComparable<TValue>
    where TSqlValue : SqlGenericValue<SqlDateTimeValueRange, TValue>
    {
        protected SqlGenericDateTimeTypeHandler(SqlGenericDateTimeTypeValueFactory<TValue, TSqlValue> valueFactory, ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : base(valueFactory, typeConverter, violations)
        {
        }

        public override SqlDateTimeValueRange CombineSize(SqlDateTimeValueRange a, SqlDateTimeValueRange b)
        {
            var rangeMin = a.Low < b.Low ? a.Low : b.Low;
            var rangeMax = a.High > b.High ? a.High : b.High;

            return new SqlDateTimeValueRange(rangeMin, rangeMax);
        }
    }
}
