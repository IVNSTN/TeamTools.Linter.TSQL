using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class LimitedIntResultFunctionHandler : SqlZeroArgFunctionHandler
    {
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.Int;
        private readonly SqlIntValueRange resultRange;

        protected LimitedIntResultFunctionHandler(string funcName, SqlIntValueRange resultRange)
        : this(funcName, resultRange, ResultTypeName)
        {
        }

        protected LimitedIntResultFunctionHandler(string funcName, SqlIntValueRange resultRange, string resultType)
        : base(funcName, resultType)
        {
            this.resultRange = resultRange;
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<ZeroArgs> call)
        {
            var value = call.Context.Converter.ImplicitlyConvert<SqlIntTypeValue>(base.DoEvaluateResultValue(call));
            if (value is null)
            {
                return default;
            }

            return value.ChangeTo(resultRange, value.Source);
        }
    }
}
