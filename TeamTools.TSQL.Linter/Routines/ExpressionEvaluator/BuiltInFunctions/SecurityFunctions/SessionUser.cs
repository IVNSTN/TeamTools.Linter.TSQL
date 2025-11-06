namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SecurityFunctions
{
    public class SessionUser : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "SESSION_USER";
        private static readonly string ResultTypeName = "dbo.SYSNAME";

        public SessionUser() : base(FuncName, ResultTypeName)
        {
        }
    }
}
