using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.Linter.Routines
{
    internal abstract class BaseSingleLineCommentValidationRule : AbstractRule
    {
        protected BaseSingleLineCommentValidationRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            for (int i = node.ScriptTokenStream.Count - 1; i >= 0; i--)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.SingleLineComment
                && !IsValidCommentFormat(token.Text))
                {
                    HandleTokenError(token);
                }
            }
        }

        protected abstract bool IsValidCommentFormat(string text);
    }
}
