using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class TranCount : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "@@TRANCOUNT";
        private static readonly SqlIntValueRange TranCountRange = new SqlIntValueRange(0, int.MaxValue);

        public TranCount() : base(FuncName, TranCountRange)
        {
        }
    }
}
