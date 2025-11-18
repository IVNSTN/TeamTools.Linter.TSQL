using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class FetchStatus : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "@@FETCHSTATUS";

        // https://learn.microsoft.com/en-us/sql/t-sql/functions/fetch-status-transact-sql
        private static readonly SqlIntValueRange FetchRange = new SqlIntValueRange(-9, 0);

        public FetchStatus() : base(FuncName, FetchRange)
        {
        }
    }
}
