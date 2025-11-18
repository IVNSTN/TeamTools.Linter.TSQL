using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0262", "ML_COMMENT_OPEN_CLOSE_WHITESPACE")]
    internal sealed class MultilineCommentOpenCloseWhitespaceRule : AbstractRule
    {
        // 1 space before, 1 space after or no text near tag
        private static readonly Regex WhitespacesOpen = new Regex("^(?<open_tag>/[*])(\\s{1}[^\\s]|\\s*$)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private static readonly Regex WhitespacesClose = new Regex("([^\\s][\\s]{1}|^\\s*)(?<close_tag>[*]/)$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        public MultilineCommentOpenCloseWhitespaceRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            for (int i = node.ScriptTokenStream.Count - 1; i >= 0; i--)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.MultilineComment
                && !IsValidCommentFormat(token.Text))
                {
                    HandleTokenError(token);
                }
            }
        }

        private static bool IsValidCommentFormat(string commentText)
        {
            // TODO : less string manufacturing
            var lines = commentText.Split(Environment.NewLine);

            // TODO : no need in regex here
            return WhitespacesOpen.IsMatch(lines[0])
                && WhitespacesClose.IsMatch(lines[lines.Length - 1]);
        }
    }
}
