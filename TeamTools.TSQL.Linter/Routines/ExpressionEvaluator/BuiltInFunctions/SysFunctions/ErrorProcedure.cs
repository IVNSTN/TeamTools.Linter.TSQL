namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ErrorProcedure : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "ERROR_PROCEDURE";
        private static readonly string ResultTypeName = "dbo.SYSNAME";

        public ErrorProcedure() : base(FuncName, ResultTypeName)
        {
        }
    }
}
