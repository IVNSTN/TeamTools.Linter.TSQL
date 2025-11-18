using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class Identity : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@IDENTITY";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.Int; // TODO : actually it's BIGINT

        public Identity() : base(FuncName, ResultTypeName)
        {
        }
    }
}
