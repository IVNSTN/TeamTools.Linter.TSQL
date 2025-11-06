using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ErrorSeverity : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "ERROR_SEVERITY";
        private static readonly SqlIntValueRange SeverityRange = new SqlIntValueRange(0, 25);

        public ErrorSeverity() : base(FuncName, SeverityRange)
        {
        }
    }
}
