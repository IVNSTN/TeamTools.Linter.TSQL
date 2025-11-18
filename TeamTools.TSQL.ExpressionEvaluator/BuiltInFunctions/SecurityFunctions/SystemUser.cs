using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SecurityFunctions
{
    public class SystemUser : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "SYSTEM_USER";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SysName;

        public SystemUser() : base(FuncName, ResultTypeName)
        {
        }
    }
}
