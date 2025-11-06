namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ScopeIdentity : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "SCOPE_IDENTITY";
        private static readonly string ResultTypeName = "dbo.INT";  // TODO : actually it's NUMERIC

        public ScopeIdentity() : base(FuncName, ResultTypeName)
        {
        }
    }
}
