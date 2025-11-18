using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0260", "JOIN_PARENTHESIS_ALIGN")]
    internal sealed class JoinParenthesisAlignedToFromRule : AbstractRule
    {
        public JoinParenthesisAlignedToFromRule() : base()
        {
        }

        public override void Visit(FromClause node)
        {
            int n = node.TableReferences.Count;
            for (int i = 0; i < n; i++)
            {
                if (!(node.TableReferences[i] is JoinTableReference join))
                {
                    continue;
                }

                if (join.FirstTableReference is QueryDerivedTable)
                {
                    DoValidateParenthesisAlign(node, join.SecondTableReference);
                }

                if (join.SecondTableReference is QueryDerivedTable)
                {
                    DoValidateParenthesisAlign(node, join.SecondTableReference);
                }
            }
        }

        private static bool ValidateParenthesisAlign(FromClause from, TSqlFragment join)
        {
            if (from.StartLine == join.StartLine)
            {
                // ignoring one-line statement
                return true;
            }

            int i = join.FirstTokenIndex;
            int n = join.LastTokenIndex;
            while (i <= n && join.ScriptTokenStream[i].TokenType != TSqlTokenType.LeftParenthesis)
            {
                i++;
            }

            int joinOpenCol = join.ScriptTokenStream[i].Column;

            bool isSingleLine = true;
            n = join.LastTokenIndex;
            while (i <= n)
            {
                if (join.ScriptTokenStream[i].Line > join.StartLine)
                {
                    isSingleLine = false;
                    break;
                }

                i++;
            }

            if (isSingleLine)
            {
                // ignoring one-line expression
                return true;
            }

            // close parenthesis alignment is supposed to be controlled with OpenCloseParenthesisAlignRule
            return joinOpenCol == from.StartColumn;
        }

        private void DoValidateParenthesisAlign(FromClause from, TSqlFragment join)
        {
            if (!ValidateParenthesisAlign(from, join))
            {
                HandleNodeError(join);
            }
        }
    }
}
