using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0242", "ML_COMMENT_ASTERISK_COUNT")]
    internal sealed class RedundantAsteriskInMultilineCommentBlockRule : AbstractRule
    {
        public RedundantAsteriskInMultilineCommentBlockRule() : base()
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
                    if (!ValidateCommentFormat(node.ScriptTokenStream[i].Text))
                    {
                        HandleTokenError(node.ScriptTokenStream[i]);
                    }
                }
            }
        }

        private bool ValidateCommentFormat(string script)
        {
            if (Regex.Match(script.Replace(" ", ""), "^/\\*([*]{1,}(?!(/$)))").Success)
            {
                return false;
            }

            if (Regex.Match(script.Replace(" ", ""), "((?<!(^/))[*]{1,})\\*/$").Success)
            {
                return false;
            }

            return true;
        }
    }
}
