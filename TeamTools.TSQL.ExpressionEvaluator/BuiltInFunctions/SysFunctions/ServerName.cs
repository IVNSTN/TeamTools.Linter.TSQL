using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ServerName : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@SERVERNAME";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SysName;

        public ServerName() : base(FuncName, ResultTypeName)
        {
        }
    }
}
