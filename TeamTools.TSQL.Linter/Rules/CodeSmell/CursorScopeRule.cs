using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0195", "CURSOR_LOCAL")]
    [CursorRule]
    internal sealed class CursorScopeRule : BaseCursorDefinitionRule
    {
        public CursorScopeRule() : base()
        {
        }

        protected override void ValidateCursor(string cursorName, CursorDefinition node)
        {
            if (!node.Options.HasOption(CursorOptionKind.Local))
            {
                HandleTokenError(TokenLocator.LocateFirstBeforeOrDefault(node, TSqlTokenType.Cursor), cursorName);
            }
        }
    }
}
