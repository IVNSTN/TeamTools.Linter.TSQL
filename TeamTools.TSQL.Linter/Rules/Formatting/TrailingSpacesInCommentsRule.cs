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

        protected override void ValidateScript(TSqlScript node)
        {
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                if ((token.TokenType == TSqlTokenType.SingleLineComment)
                || token.TokenType == TSqlTokenType.MultilineComment)
                {
                    if (!ValidateTrailingSpaces(token.Text))
                    {
                        HandleTokenError(token);
                    }
                }
            }
        }

        // TODO : get rid of string splitting
        private static bool ValidateTrailingSpaces(string comment)
        {
            string[] lines = comment.Split(Environment.NewLine);
            int n = lines.Length;
            for (int j = 0; j < n; j++)
            {
                if (lines[j].EndsWith(" "))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
