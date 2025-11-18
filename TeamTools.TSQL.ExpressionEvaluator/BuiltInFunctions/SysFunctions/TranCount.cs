using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
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
