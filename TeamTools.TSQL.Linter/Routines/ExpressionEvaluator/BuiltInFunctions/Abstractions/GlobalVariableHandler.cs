namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions
{
    public abstract class GlobalVariableHandler : SqlZeroArgFunctionHandler
    {
        public GlobalVariableHandler(string funcName, string resultTypeName) : base(funcName, resultTypeName)
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
