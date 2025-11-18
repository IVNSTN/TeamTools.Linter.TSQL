using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class XactState : LimitedIntResultFunctionHandler
    {
        private static readonly string FuncName = "XACT_STATE";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SmallInt;
        private static readonly SqlIntValueRange XactRange = new SqlIntValueRange(-1, 1);

        public XactState() : base(FuncName, XactRange, ResultTypeName)
        {
        }
    }
}
