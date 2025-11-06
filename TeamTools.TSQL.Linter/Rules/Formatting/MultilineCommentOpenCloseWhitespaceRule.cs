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
        private readonly Regex whitespacesOpen = new Regex("^(?<open_tag>/[*])(\\s{1}[^\\s]|\\s*$)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        private readonly Regex whitespacesClose = new Regex("([^\\s][\\s]{1}|^\\s*)(?<close_tag>[*]/)$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        public MultilineCommentOpenCloseWhitespaceRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if (node.ScriptTokenStream[i].TokenType != TSqlTokenType.MultilineComment)
                {
                    continue;
                }

                var lines = node.ScriptTokenStream[i].Text.Split(Environment.NewLine);

                if (!whitespacesOpen.IsMatch(lines[0]) || !whitespacesClose.IsMatch(lines[lines.Length - 1]))
                {
                    HandleTokenError(node.ScriptTokenStream[i]);
                }
            }
        }
    }
}
