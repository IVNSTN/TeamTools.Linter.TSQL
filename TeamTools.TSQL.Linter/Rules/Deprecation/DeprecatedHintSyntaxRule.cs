using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0273", "HINT_SYNTAX")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class DeprecatedHintSyntaxRule : AbstractRule
    {
        public DeprecatedHintSyntaxRule() : base()
        {
        }

        public void ValidateWithKeyword(TSqlFragment firstHint)
        {
            int i = firstHint.FirstTokenIndex - 1;
            while (i > 0 && (firstHint.ScriptTokenStream[i].TokenType == TSqlTokenType.WhiteSpace || firstHint.ScriptTokenStream[i].TokenType == TSqlTokenType.LeftParenthesis))
            {
                i--;
            }

            if (firstHint.ScriptTokenStream[i].TokenType == TSqlTokenType.With)
            {
                return;
            }

            HandleNodeError(firstHint);
        }

        public void ValidateCommasBetween(IList<TableHint> hints)
        {
            int n = hints.Count;
            for (int i = 1; i < n; i++)
            {
                int priorEnd = hints[i - 1].LastTokenIndex;
                int nextStart = hints[i].FirstTokenIndex;

                int j = priorEnd + 1;
                while (j < nextStart && hints[i].ScriptTokenStream[j].TokenType != TSqlTokenType.Comma)
                {
                    j++;
                }

                if (hints[i].ScriptTokenStream[j].TokenType != TSqlTokenType.Comma)
                {
                    HandleLineError(hints[i].ScriptTokenStream[j].Line, hints[i].ScriptTokenStream[j].Column);
                }
            }
        }

        public override void Visit(NamedTableReference node)
        {
            if (node.TableHints.Count == 0)
            {
                return;
            }

            ValidateWithKeyword(node.TableHints[0]);

            if (node.TableHints.Count == 1)
            {
                return;
            }

            ValidateCommasBetween(node.TableHints);
        }
    }
}
