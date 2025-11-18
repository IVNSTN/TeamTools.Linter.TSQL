using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SecurityFunctions
{
    public class SessionUser : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "SESSION_USER";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SysName;

        public SessionUser() : base(FuncName, ResultTypeName)
        {
        }
    }
}
