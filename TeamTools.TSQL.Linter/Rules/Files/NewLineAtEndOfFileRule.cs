using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0306", "EOF_NEWLINE")]
    internal sealed class NewLineAtEndOfFileRule : AbstractRule
    {
        private static readonly char[] TrimmedChars = new char[] { '\r', '\n' };

        public NewLineAtEndOfFileRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            int i = node.LastTokenIndex;
            int start = node.FirstTokenIndex;
            int newLineCount = 0;

            if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.EndOfFile)
            {
                i--;
            }

            if (i < start)
            {
                return;
            }

            while ((i >= start) && (newLineCount < 1)
                && (node.ScriptTokenStream[i].TokenType == TSqlTokenType.WhiteSpace)
                && string.IsNullOrEmpty(node.ScriptTokenStream[i].Text.Trim(TrimmedChars)))
            {
                newLineCount++;
                i--;
            }

            if (newLineCount >= 1)
            {
                return;
            }

            HandleTokenError(node.ScriptTokenStream[i]);
        }
    }
}
