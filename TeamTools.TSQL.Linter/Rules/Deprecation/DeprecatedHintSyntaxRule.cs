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
            TSqlParserToken token = null;
            for (int i = firstHint.FirstTokenIndex - 1; i >= 0; i--)
            {
                token = firstHint.ScriptTokenStream[i];
                if (token.TokenType != TSqlTokenType.WhiteSpace && token.TokenType != TSqlTokenType.LeftParenthesis)
                {
                    break;
                }
            }

            if (token.TokenType == TSqlTokenType.With)
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
                var hint = hints[i];
                int priorEnd = hints[i - 1].LastTokenIndex;
                int nextStart = hint.FirstTokenIndex;

                int j = priorEnd + 1;
                while (j < nextStart && hint.ScriptTokenStream[j].TokenType != TSqlTokenType.Comma)
                {
                    j++;
                }

                var token = hint.ScriptTokenStream[j];
                if (token.TokenType != TSqlTokenType.Comma)
                {
                    HandleLineError(token.Line, token.Column);
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
