using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ErrorState : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "ERROR_STATE";
        private static readonly SqlIntValueRange StateRange = new SqlIntValueRange(0, 255);

        public ErrorState() : base(FuncName, StateRange)
        {
        }
    }
}
