using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class Error : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@ERROR";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.Int;

        public Error() : base(FuncName, ResultTypeName)
        {
        }
    }
}
