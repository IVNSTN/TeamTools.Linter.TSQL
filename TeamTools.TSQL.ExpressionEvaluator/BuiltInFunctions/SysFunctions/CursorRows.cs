using TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.Abstractions;
using TeamTools.TSQL.ExpressionEvaluator.Routines;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.ExpressionEvaluator.BuiltInFunctions.SysFunctions
{
    public class CursorRows : GlobalVariableHandler
    {
        private static readonly string FuncName = "@@CURSOR_ROWS";
        private static readonly string ResultTypeName = TSqlDomainAttributes.Types.Int;

        public CursorRows() : base(FuncName, ResultTypeName)
        {
        }
    }
}
