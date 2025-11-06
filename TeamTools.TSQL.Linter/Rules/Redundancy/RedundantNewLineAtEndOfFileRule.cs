using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0307", "EOF_REDUNDANT_NEWLINE")]
    internal sealed class RedundantNewLineAtEndOfFileRule : AbstractRule
    {
        public RedundantNewLineAtEndOfFileRule() : base()
        {
        }

        // TODO : this is very similar to NewLineAtEndOfFileRule
        public override void Visit(TSqlScript node)
        {
            int i = node.LastTokenIndex;
            int newLineCount = 0;

            if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.EndOfFile)
            {
                i--;
            }

            if (i < node.FirstTokenIndex)
            {
                return;
            }

            int start = node.FirstTokenIndex;
            while ((i >= start) && (newLineCount <= 1)
            && (node.ScriptTokenStream[i].TokenType == TSqlTokenType.WhiteSpace))
            {
                newLineCount += Regex.Split(string.Format(" {0} ", node.ScriptTokenStream[i].Text), "\r\n|\r|\n").Length - 1;
                i--;
            }

            if (newLineCount <= 1)
            {
                return;
            }

            HandleLineError(node.GetLastReadableLine(node.LastTokenIndex), 0);
        }
    }
}
