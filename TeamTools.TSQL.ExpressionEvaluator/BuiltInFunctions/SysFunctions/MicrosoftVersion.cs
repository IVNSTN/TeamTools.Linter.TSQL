using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class MicrosoftVersion : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@MICROSOFTVERSION";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.Int;

        // TODO : Report on undocumented feature call?
        public MicrosoftVersion() : base(FuncName, ResultTypeName)
        {
        }
    }
}
