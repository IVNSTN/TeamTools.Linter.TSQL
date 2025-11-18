using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class ErrorProcedure : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "ERROR_PROCEDURE";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SysName;

        public ErrorProcedure() : base(FuncName, ResultTypeName)
        {
        }
    }
}
