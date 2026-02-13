using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class CurrentMomentFunction<TOut> : SqlZeroArgFunctionHandler
    where TOut : SqlValue
    {
        private readonly TimeDetails timeDetails;
        private readonly DateDetails dateDetails;

        protected CurrentMomentFunction(string funcName, string outputType, TimeDetails timeDetails, DateDetails dateDetails) : base(funcName, outputType)
        {
            this.timeDetails = timeDetails;
            this.dateDetails = dateDetails;
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<ZeroArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<TOut>(base.DoEvaluateResultValue(call));

            if (value is null)
            {
                return default;
            }

            var newRange = new SqlDateTimeValueRange(new SqlDateTimeRelativeValue(DateTimeRangeKind.CurrentMoment, timeDetails, dateDetails));

            return ApplyNewRange(value, newRange, call.Context.NewSource);
        }

        protected abstract TOut ApplyNewRange(TOut value, SqlDateTimeValueRange newRange, SqlValueSource src);
    }
}
