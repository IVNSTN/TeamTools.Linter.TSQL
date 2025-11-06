namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SecurityFunctions
{
    public class OriginalLogin : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "ORIGINAL_LOGIN";
        private static readonly string ResultTypeName = "dbo.SYSNAME";

        public OriginalLogin() : base(FuncName, ResultTypeName)
        {
        }
    }
}
