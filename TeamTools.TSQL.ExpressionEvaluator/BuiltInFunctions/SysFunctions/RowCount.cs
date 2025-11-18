using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class RowCount : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "@@ROWCOUNT";
        private static readonly SqlIntValueRange RowCountRange = new SqlIntValueRange(0, int.MaxValue);

        public RowCount() : base(FuncName, RowCountRange)
        {
        }
    }
}
