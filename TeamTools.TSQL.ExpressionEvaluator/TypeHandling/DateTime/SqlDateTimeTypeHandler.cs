using System;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces.OperatorHandlers;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.TypeHandling
{
    public sealed class SqlDateTimeTypeHandler : SqlGenericDateTimeTypeHandler<DateTime, SqlDateTimeValue>,
        IPlusOperatorHandler, IMinusOperatorHandler
    {
        private readonly SqlDateTimeTypeValueFactory typedValueFactory;
        private readonly Func<DateTime, DateTime, DateTime> getSum;
        private readonly Func<DateTime, DateTime, DateTime> getSubtract;
        private readonly Func<SqlDateTimeValueRange, SqlDateTimeValueRange, SqlDateTimeValueRange> makeApproxSum;

        public SqlDateTimeTypeHandler(ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : this(new SqlDateTimeTypeValueFactory(), typeConverter, violations)
        {
        }

        private SqlDateTimeTypeHandler(SqlDateTimeTypeValueFactory valueFactory, ISqlTypeConverter typeConverter, IViolationRegistrar violations)
        : base(valueFactory, typeConverter, violations)
        {
            valueFactory.TypeHandler = this;
            typedValueFactory = valueFactory;

            // FIXME : но если в конвертере целое число переводить в дату с прибавкой 1900-01-01, то здесь же нужно тогда это учитывать
            getSum = new Func<DateTime, DateTime, DateTime>((a, b) => a.Add(new TimeSpan(b.Ticks - SqlDateTimeTypeReference.MinSqlValue.Ticks)));
            getSubtract = new Func<DateTime, DateTime, DateTime>((a, b) => a.Subtract(new TimeSpan(b.Ticks - SqlDateTimeTypeReference.MinSqlValue.Ticks)));
            // TODO : does this actually make sense?
            makeApproxSum = new Func<SqlDateTimeValueRange, SqlDateTimeValueRange, SqlDateTimeValueRange>((sizeA, sizeB) => new SqlDateTimeValueRange(
                sizeA.Low < sizeB.Low ? sizeA.Low : sizeB.Low,
                sizeA.High > sizeB.High ? sizeA.High : sizeB.High));
        }

        public SqlDateTimeTypeValueFactory DateTimeValueFactory => typedValueFactory;

        public override ISqlValueFactory GetValueFactory() => typedValueFactory;

        public override SqlTypeReference MakeSqlDataTypeReference(string typeName)
            => typedValueFactory.MakeSqlDataTypeReference(typeName);

        public override SqlValue ConvertFrom(SqlValue from, SqlTypeReference to, bool forceTargetType = false)
            => to is SqlDateTimeTypeReference datetimeType ? ConvertFrom(from, datetimeType, forceTargetType) : default;

        public SqlDateTimeValue ConvertFrom(SqlValue from, SqlDateTimeTypeReference to, bool forceTargetType)
            => this.ConvertValueFrom(from, to, forceTargetType);

        public override SqlValue ConvertFrom(SqlValue from, string to)
            => IsTypeSupported(to) ? this.ConvertValueFrom(from, to) : default;

        public SqlValue Sum(SqlValue augend, SqlValue addend)
        {
            return Compute(
                augend,
                addend,
                getSum,
                makeApproxSum);
        }

        public SqlValue Subtract(SqlValue minuend, SqlValue subtrahend)
        {
            return Compute(
                minuend,
                subtrahend,
                getSubtract,
                makeApproxSum);
        }
    }
}
