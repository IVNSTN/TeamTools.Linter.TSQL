namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class Identity : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@IDENTITY";
        private static readonly string ResultTypeName = "dbo.INT"; // TODO : actually it's BIGINT

        public Identity() : base(FuncName, ResultTypeName)
        {
        }
    }
}
