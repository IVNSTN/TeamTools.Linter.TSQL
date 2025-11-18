using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0242", "ML_COMMENT_ASTERISK_COUNT")]
    internal sealed class RedundantAsteriskInMultilineCommentBlockRule : AbstractRule
    {
        public RedundantAsteriskInMultilineCommentBlockRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            // '/**/' - is already 4
            const int MinLengthToHaveSomethingRedundant = 5;

            for (int i = node.ScriptTokenStream.Count - 1; i >= 0; i--)
            {
                var comment = node.ScriptTokenStream[i];

                if (comment.TokenType != TSqlTokenType.MultilineComment)
                {
                    continue;
                }

                if (comment.Text.Length >= MinLengthToHaveSomethingRedundant
                && !IsValidCommentFormat(comment.Text))
                {
                    HandleTokenError(comment);
                }
            }
        }

        private static bool IsValidCommentFormat(string comment) => IsValidStart(comment) && IsValidEnd(comment);

        private static bool IsValidStart(string comment)
        {
            int n = comment.Length - 2; // to ignore closing '*/' here
            int i = 1; // skipping first '/'
            int asteriskCount = 0;

            while (i < n && asteriskCount < 2)
            {
                var c = comment[i];
                if (c == '*')
                {
                    asteriskCount++;
                }
                else if (c == '/' && asteriskCount == 0)
                {
                    // just skipping all '/' at the beginning
                }
                else if (!char.IsWhiteSpace(c))
                {
                    // some text found
                    return true;
                }

                i++;
            }

            return asteriskCount == 1;
        }

        private static bool IsValidEnd(string comment)
        {
            int i = comment.Length - 2; // skipping last '/'
            int asteriskCount = 0;

            while (i > 1 && asteriskCount < 2)
            {
                var c = comment[i];
                if (c == '*')
                {
                    asteriskCount++;
                }
                else if (c == '/' && asteriskCount == 0)
                {
                    // just skipping all '/' at the end
                }
                else if (!char.IsWhiteSpace(c))
                {
                    // some text found
                    return true;
                }

                i--;
            }

            return asteriskCount == 1;
        }
    }
}
