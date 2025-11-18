using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
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
            if (IsReadonlyCursor(node.Options))
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

        private static bool IsReadonlyCursor(IList<CursorOption> options)
        {
            for (int i = 0, n = options.Count; i < n; i++)
            {
                var opt = options[i];

                // FAST_FORWARD includes READ_ONLY
                if (opt.OptionKind == CursorOptionKind.ReadOnly
                || opt.OptionKind == CursorOptionKind.FastForward)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
