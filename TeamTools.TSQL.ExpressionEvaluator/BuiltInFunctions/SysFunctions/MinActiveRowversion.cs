using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class MinActiveRowversion : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "MIN_ACTIVE_ROWVERSION ";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.Binary;

        // TODO : set size = 8
        public MinActiveRowversion() : base(FuncName, ResultTypeName)
        {
        }
    }
}
