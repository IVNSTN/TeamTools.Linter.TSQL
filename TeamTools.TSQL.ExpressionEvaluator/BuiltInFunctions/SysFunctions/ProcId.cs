using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ProcId : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@PROCID";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.Int;

        public ProcId() : base(FuncName, ResultTypeName)
        {
        }
    }
}
