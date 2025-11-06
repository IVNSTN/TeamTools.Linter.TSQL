using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0233", "TRAILING_SPACES_IN_COMMENT")]
    internal sealed class TrailingSpacesInCommentsRule : AbstractRule
    {
        public TrailingSpacesInCommentsRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if ((node.ScriptTokenStream[i].TokenType == TSqlTokenType.SingleLineComment)
                || node.ScriptTokenStream[i].TokenType == TSqlTokenType.MultilineComment)
                {
                    if (!ValidateTrailingSpaces(node.ScriptTokenStream[i].Text))
                    {
                        HandleTokenError(node.ScriptTokenStream[i]);
                    }
                }
            }
        }

        private static bool ValidateTrailingSpaces(string comment)
        {
            string[] lines = comment.Split(Environment.NewLine);
            int n = lines.Length;
            for (int j = 0; j < n; j++)
            {
                if (lines[j].TrimEnd() != lines[j])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
