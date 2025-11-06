using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0229", "WHITESPACE_IN_NEWLINE")]
    internal sealed class WhitespaceInEmptyLineRule : AbstractRule
    {
        public WhitespaceInEmptyLineRule() : base()
        {
        }

        // TODO : simplify, optimize
        public override void Visit(TSqlScript node)
        {
            int n = node.ScriptTokenStream.Count;
            for (int i = 0; i < n; i++)
            {
                if ((node.ScriptTokenStream[i].TokenType != TSqlTokenType.WhiteSpace)
                && (node.ScriptTokenStream[i].TokenType != TSqlTokenType.MultilineComment))
                {
                    continue;
                }

                // if next in the line is something else than whitespace, than this is not the case
                if (((i + 1) < node.ScriptTokenStream.Count)
                && (node.ScriptTokenStream[i + 1].Line == node.ScriptTokenStream[i].Line)
                && (node.ScriptTokenStream[i + 1].TokenType != TSqlTokenType.WhiteSpace))
                {
                    continue;
                }

                // if prior in the line is something else than whitespace, than this is not the case
                if ((i > 0)
                && (node.ScriptTokenStream[i - 1].Line == node.ScriptTokenStream[i].Line)
                && (node.ScriptTokenStream[i - 1].TokenType != TSqlTokenType.WhiteSpace))
                {
                    continue;
                }

                string script = node.ScriptTokenStream[i].Text;

                if (string.IsNullOrEmpty(script) || (Environment.NewLine == script))
                {
                    continue;
                }

                DetectWhitespaceInLines(
                    Regex.Split(script, "\r\n|\r|\n"),
                    node.ScriptTokenStream[i].Line);
            }
        }

        private void DetectWhitespaceInLines(string[] lines, int startLine)
        {
            int n = lines.Length;
            // linebreaks are treated as separate tokens thus starting with 0 and reflecting each line of spaces
            for (int j = 0; j < n; j++)
            {
                if ((lines[j].Length > 0) && (lines[j].Trim().Length == 0))
                {
                    HandleLineError(startLine + j, 0);
                }
            }
        }
    }
}
