using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SecurityFunctions
{
    public class CurrentUser : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "CURRENT_USER";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SysName;

        public CurrentUser() : base(FuncName, ResultTypeName)
        {
        }
    }
}
