using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ErrorLine : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "ERROR_LINE";
        private static readonly SqlIntValueRange LineRange = new SqlIntValueRange(1, int.MaxValue);

        public ErrorLine() : base(FuncName, LineRange)
        {
        }
    }
}
