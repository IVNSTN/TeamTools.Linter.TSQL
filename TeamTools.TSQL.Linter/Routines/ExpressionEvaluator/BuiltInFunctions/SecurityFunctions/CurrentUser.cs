namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SecurityFunctions
{
    public class CurrentUser : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "CURRENT_USER";
        private static readonly string ResultTypeName = "dbo.SYSNAME";

        public CurrentUser() : base(FuncName, ResultTypeName)
        {
        }
    }
}
