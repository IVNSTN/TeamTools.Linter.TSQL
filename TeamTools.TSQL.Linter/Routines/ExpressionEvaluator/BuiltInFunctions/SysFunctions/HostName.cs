namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class HostName : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "HOST_NAME";
        private static readonly string ResultTypeName = "dbo.SYSNAME";

        public HostName() : base(FuncName, ResultTypeName)
        {
        }
    }
}
