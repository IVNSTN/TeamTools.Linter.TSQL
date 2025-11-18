using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ErrorNumber : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "ERROR_NUMBER";

        // https://learn.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors-0-to-999
        private static readonly SqlIntValueRange ErrorRange = new SqlIntValueRange(21, int.MaxValue);

        public ErrorNumber() : base(FuncName, ErrorRange)
        {
        }
    }
}
