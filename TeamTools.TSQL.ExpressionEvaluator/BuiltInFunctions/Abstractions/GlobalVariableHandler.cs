using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.ArgumentDto;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions
{
    public abstract class GlobalVariableHandler : SqlZeroArgFunctionHandler
    {
        protected GlobalVariableHandler(string funcName, string resultTypeName) : base(funcName, resultTypeName)
        {
        }

        protected override SqlValue DoEvaluateResultValue(CallSignature<ZeroArgs> call)
        {
            var localOverride = call.Context.Variables.GetValueAt(FunctionName, call.Context.Node.FirstTokenIndex);
            if (localOverride != null && localOverride.IsPreciseValue)
            {
                return localOverride;
            }

            return base.DoEvaluateResultValue(call);
        }
    }
}
