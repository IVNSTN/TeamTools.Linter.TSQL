namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ServerName : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@SERVERNAME";
        private static readonly string ResultTypeName = "dbo.SYSNAME";

        public ServerName() : base(FuncName, ResultTypeName)
        {
        }
    }
}
