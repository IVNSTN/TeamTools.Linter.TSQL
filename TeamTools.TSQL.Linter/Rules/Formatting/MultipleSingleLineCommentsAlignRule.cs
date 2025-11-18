using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0246", "MANY_SL_COMMENTS_ALIGNED")]
    internal sealed class MultipleSingleLineCommentsAlignRule : AbstractRule
    {
        public MultipleSingleLineCommentsAlignRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            int lastCommentColumn = -1;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.WhiteSpace)
                {
                    // do nothing
                }
                else if (token.TokenType == TSqlTokenType.SingleLineComment)
                {
                    if ((lastCommentColumn >= 0) && (lastCommentColumn != token.Column))
                    {
                        HandleTokenError(token);
                    }

                    lastCommentColumn = token.Column;
                }
                else
                {
                    lastCommentColumn = -1;
                }
            }
        }
    }
}
