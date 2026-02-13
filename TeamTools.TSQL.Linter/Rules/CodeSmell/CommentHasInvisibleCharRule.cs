using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // See also IdentifierHasInvisibleCharRule, LiteralHasInvisibleCharRule
    [RuleIdentity("CS0842", "INVISIBLE_CHAR_IN_COMMENT")]
    internal sealed class CommentHasInvisibleCharRule : AbstractRule
    {
        public CommentHasInvisibleCharRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            for (int i = 0, n = node.ScriptTokenStream.Count; i < n; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.SingleLineComment || token.TokenType == TSqlTokenType.MultilineComment)
                {
                    ValidateChars(token.Text, token.Line, token.Column);
                }
            }
        }

        private void ValidateChars(string text, int line, int col)
        {
            int badCharPos = InvisibleCharDetector.LocateInvisibleChar(text, out string symbolName);
            if (badCharPos >= 0)
            {
                // TODO: point to exact line and col of multiline comment
                HandleLineError(line, col, symbolName);
            }
        }
    }
}
