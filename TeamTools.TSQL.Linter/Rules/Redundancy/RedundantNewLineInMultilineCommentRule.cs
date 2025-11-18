using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0261", "ML_COMMENT_REDUNDANT_NEWLINE")]
    internal sealed class RedundantNewLineInMultilineCommentRule : AbstractRule
    {
        private readonly int maxEmptyLines = 2;

        public RedundantNewLineInMultilineCommentRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            for (int i = node.ScriptTokenStream.Count - 1; i >= 0; i--)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.MultilineComment)
                {
                    if (!ValidateCommentEmptyLines(token.Text))
                    {
                        HandleTokenError(token);
                    }
                }
            }
        }

        private bool ValidateCommentEmptyLines(string commentText)
        {
            if (string.IsNullOrEmpty(commentText))
            {
                return true;
            }

            // TODO : less string manufacturing
            var commentLines = commentText.Split(Environment.NewLine);
            if (commentLines.Length <= maxEmptyLines)
            {
                return true;
            }

            int emptyLines = 0;
            int n = commentLines.Length;

            for (int i = 0; i < n; i++)
            {
                var line = commentLines[i];
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(line) || trimmedLine.Equals("/*") || trimmedLine.Equals("*/"))
                {
                    emptyLines++;
                }
                else
                {
                    emptyLines = 0;
                }

                if (emptyLines > maxEmptyLines)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
