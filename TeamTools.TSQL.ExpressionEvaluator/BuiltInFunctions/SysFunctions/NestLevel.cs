using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class NestLevel : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "@@NESTLEVEL";
        // https://learn.microsoft.com/en-us/sql/t-sql/functions/nestlevel-transact-sql
        private static readonly SqlIntValueRange NestLevelRange = new SqlIntValueRange(0, 32);

        public NestLevel() : base(FuncName, NestLevelRange)
        {
        }
    }
}
