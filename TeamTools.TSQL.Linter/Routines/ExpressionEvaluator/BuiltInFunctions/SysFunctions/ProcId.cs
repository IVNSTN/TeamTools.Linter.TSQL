namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ProcId : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@PROCID";
        private static readonly string ResultTypeName = "dbo.INT";

        public ProcId() : base(FuncName, ResultTypeName)
        {
        }
    }
}
