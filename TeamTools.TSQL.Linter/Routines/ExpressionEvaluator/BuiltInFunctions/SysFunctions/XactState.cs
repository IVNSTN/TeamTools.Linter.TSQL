using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class XactState : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "XACT_STATE";
        private static readonly string ResultTypeName = "dbo.SMALLINT";
        private static readonly SqlIntValueRange XactRange = new SqlIntValueRange(-1, 1);

        public XactState() : base(FuncName, XactRange, ResultTypeName)
        {
        }
    }
}
