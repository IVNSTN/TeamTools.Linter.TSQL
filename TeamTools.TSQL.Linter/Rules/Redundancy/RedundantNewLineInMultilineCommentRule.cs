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

        public override void Visit(TSqlScript node)
        {
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.MultilineComment)
                {
                    if (!ValidateCommentEmptyLines(node.ScriptTokenStream[i].Text))
                    {
                        HandleTokenError(node.ScriptTokenStream[i]);
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

            var commentLines = commentText.Split(Environment.NewLine);
            if (commentLines.Length <= maxEmptyLines)
            {
                return true;
            }

            int emptyLines = 0;
            int n = commentLines.Length;

            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrWhiteSpace(commentLines[i]) || commentLines[i].Trim().Equals("/*") || commentLines[i].Trim().Equals("*/"))
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
