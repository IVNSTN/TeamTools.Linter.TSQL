namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class Error : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@ERROR";
        private static readonly string ResultTypeName = "dbo.INT";

        public Error() : base(FuncName, ResultTypeName)
        {
        }
    }
}
