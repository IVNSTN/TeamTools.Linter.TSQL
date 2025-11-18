using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0267", "TAB_IN_COMMENTS")]
    internal sealed class TabInCommentsRule : AbstractRule
    {
        public TabInCommentsRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.MultilineComment
                || token.TokenType == TSqlTokenType.SingleLineComment)
                {
                    ValidateComment(token.Text, token.Line, token.Column);
                }
            }
        }

        private void ValidateComment(string comment, int line, int col)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return;
            }

#if NETSTANDARD
            const string tab = "\t";
            if (!comment.Contains(tab))
            {
                return;
            }
#else
            if (!comment.Contains('\t'))
            {
                return;
            }
#endif

            HandleLineError(line, col);
        }
    }
}
