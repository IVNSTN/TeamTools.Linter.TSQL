using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SecurityFunctions
{
    public class OriginalLogin : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "ORIGINAL_LOGIN";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SysName;

        public OriginalLogin() : base(FuncName, ResultTypeName)
        {
        }
    }
}
