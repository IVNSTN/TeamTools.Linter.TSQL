using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0253", "OPEN_CLOSE_PARENTHESIS_ALIGNED")]
    internal sealed class OpenCloseParenthesisAlignRule : AbstractRule
    {
        public OpenCloseParenthesisAlignRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var parenthesis = new ParenthesisParser(node);
            parenthesis.Parse();

            foreach (var i in parenthesis.OpenParenthesises.Values)
            {
                if (i.CloseTokenIndex < 0)
                {
                    continue;
                }

                // one-line ignored
                if (node.ScriptTokenStream[i.OpenTokenIndex].Line == node.ScriptTokenStream[i.CloseTokenIndex].Line)
                {
                    continue;
                }

                if (node.ScriptTokenStream[i.OpenTokenIndex].Column == node.ScriptTokenStream[i.CloseTokenIndex].Column)
                {
                    continue;
                }

                // TODO : refactor
                // collapsed cte, IIF
                {
                    int j = i.CloseTokenIndex - 1;
                    int closeLine = node.ScriptTokenStream[i.CloseTokenIndex].Line;
                    bool isCollapsedBlock = false;

                    while (j > i.OpenTokenIndex && node.ScriptTokenStream[j].Line == closeLine && !isCollapsedBlock)
                    {
                        // something in the same line
                        if (node.ScriptTokenStream[j].TokenType == TSqlTokenType.WhiteSpace
                        || node.ScriptTokenStream[j].TokenType == TSqlTokenType.RightParenthesis)
                        {
                            j--;
                        }
                        else
                        {
                            isCollapsedBlock = true;
                        }
                    }

                    if (isCollapsedBlock)
                    {
                        continue;
                    }
                }

                HandleTokenError(node.ScriptTokenStream[i.CloseTokenIndex]);
            }
        }
    }
}
