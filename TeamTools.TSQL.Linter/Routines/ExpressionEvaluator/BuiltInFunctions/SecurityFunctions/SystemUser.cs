namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SecurityFunctions
{
    public class SystemUser : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "SYSTEM_USER";
        private static readonly string ResultTypeName = "dbo.SYSNAME";

        public SystemUser() : base(FuncName, ResultTypeName)
        {
        }
    }
}
