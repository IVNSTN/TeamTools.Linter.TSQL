using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ScopeIdentity : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "SCOPE_IDENTITY";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.Int;  // TODO : actually it's NUMERIC

        public ScopeIdentity() : base(FuncName, ResultTypeName)
        {
        }
    }
}
