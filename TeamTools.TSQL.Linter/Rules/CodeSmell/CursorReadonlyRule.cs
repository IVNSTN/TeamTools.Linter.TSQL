using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0196", "CURSOR_READONLY")]
    [CursorRule]
    internal sealed class CursorReadonlyRule : BaseCursorDefinitionRule
    {
        public CursorReadonlyRule() : base()
        {
        }

        protected override void ValidateCursor(string cursorName, CursorDefinition node)
        {
            // FAST_FORWARD includes READ_ONLY
            if (node.Options.Any(opt => opt.OptionKind.In(CursorOptionKind.ReadOnly, CursorOptionKind.FastForward)))
            {
                return;
            }

            if (node.Select.QueryExpression.ForClause is UpdateForClause
            || node.Select.QueryExpression.ForClause is ReadOnlyForClause)
            {
                return;
            }

            HandleTokenError(TokenLocator.LocateFirstBeforeOrDefault(node, TSqlTokenType.Cursor), cursorName);
        }
    }
}
