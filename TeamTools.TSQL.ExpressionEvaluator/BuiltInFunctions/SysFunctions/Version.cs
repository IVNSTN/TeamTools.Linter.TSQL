using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class Version : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@VERSION";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.NVarchar;

        public Version() : base(FuncName, ResultTypeName)
        {
        }
    }
}
