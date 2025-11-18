using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class HostName : SqlZeroArgFunctionHandler
    {
        private static readonly string FuncName = "HOST_NAME";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.SysName;

        public HostName() : base(FuncName, ResultTypeName)
        {
        }
    }
}
