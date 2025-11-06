using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0267", "TAB_IN_COMMENTS")]
    internal sealed class TabInCommentsRule : AbstractRule
    {
        private readonly Regex tabPresence = new Regex("\\t", RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public TabInCommentsRule() : base()
        {
        }

        public override void Visit(TSqlScript script)
        {
            int start = script.FirstTokenIndex;
            int end = script.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if (script.ScriptTokenStream[i].TokenType == TSqlTokenType.MultilineComment
                || script.ScriptTokenStream[i].TokenType == TSqlTokenType.SingleLineComment)
                {
                    ValidateComment(script.ScriptTokenStream[i].Text, script.ScriptTokenStream[i].Line, script.ScriptTokenStream[i].Column);
                }
            }
        }

        private void ValidateComment(string comment, int line, int col)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return;
            }

            if (!tabPresence.IsMatch(comment))
            {
                return;
            }

            HandleLineError(line, col);
        }
    }
}
