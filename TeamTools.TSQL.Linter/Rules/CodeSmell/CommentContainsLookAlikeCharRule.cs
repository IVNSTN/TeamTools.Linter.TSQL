using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // See also LiteralContainsLookAlikeCharRule, AlphabetMixInIdentifierRule
    [RuleIdentity("CS0835", "COMMENT_LOOK_ALIKE_CHAR")]
    internal sealed class CommentContainsLookAlikeCharRule : AbstractRule
    {
        private const int MinSingleLineCommentLength = 3;
        private const int MinMultiLineCommentLength = 5;

        public CommentContainsLookAlikeCharRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            for (int i = 0, n = node.ScriptTokenStream.Count; i < n; i++)
            {
                var token = node.ScriptTokenStream[i];
                if ((token.TokenType == TSqlTokenType.SingleLineComment && token.Text.Length > MinSingleLineCommentLength)
                || (token.TokenType == TSqlTokenType.MultilineComment && token.Text.Length > MinMultiLineCommentLength))
                {
                    ValidateChars(token.Text, token.Line, token.Column);
                }
            }
        }

        private void ValidateChars(string text, int line, int col) => LookAlikeCharDetector.ValidateChars(text, line, col, ViolationHandlerPerLine);
    }
}
