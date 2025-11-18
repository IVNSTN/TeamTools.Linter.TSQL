using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0253", "OPEN_CLOSE_PARENTHESIS_ALIGNED")]
    internal sealed class OpenCloseParenthesisAlignRule : ScriptAnalysisServiceConsumingRule
    {
        public OpenCloseParenthesisAlignRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var parenthesis = GetService<ParenthesisParser>(node);

            foreach (var i in parenthesis.OpenParenthesises.Values)
            {
                if (i.CloseTokenIndex < 0)
                {
                    continue;
                }

                var open = node.ScriptTokenStream[i.OpenTokenIndex];
                var close = node.ScriptTokenStream[i.CloseTokenIndex];

                // one-line ignored
                if (open.Line == close.Line)
                {
                    continue;
                }

                if (open.Column == close.Column)
                {
                    continue;
                }

                // TODO : refactor
                // collapsed cte, IIF
                {
                    int closeLine = close.Line;
                    bool isCollapsedBlock = false;

                    for (int j = i.CloseTokenIndex - 1, start = i.OpenTokenIndex; j > start; j--)
                    {
                        var token = node.ScriptTokenStream[j];

                        if (token.Line != closeLine)
                        {
                            break;
                        }

                        // something in the same line
                        if (!(token.TokenType == TSqlTokenType.WhiteSpace
                        || token.TokenType == TSqlTokenType.RightParenthesis))
                        {
                            isCollapsedBlock = true;
                            break;
                        }
                    }

                    if (!isCollapsedBlock)
                    {
                        HandleTokenError(close);
                    }
                }
            }
        }
    }
}
