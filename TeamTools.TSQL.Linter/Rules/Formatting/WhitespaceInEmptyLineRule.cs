using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Text.RegularExpressions;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0229", "WHITESPACE_IN_NEWLINE")]
    internal sealed class WhitespaceInEmptyLineRule : AbstractRule
    {
        private static readonly Regex SplitRegex = new Regex("\r\n|\r|\n", RegexOptions.Compiled);

        public WhitespaceInEmptyLineRule() : base()
        {
        }

        // TODO : simplify, optimize
        protected override void ValidateScript(TSqlScript node)
        {
            TSqlParserToken priorToken = default;
            int n = node.ScriptTokenStream.Count;
            for (int i = 0; i < n; i++)
            {
                var token = node.ScriptTokenStream[i];

                if ((token.TokenType != TSqlTokenType.WhiteSpace)
                && (token.TokenType != TSqlTokenType.MultilineComment))
                {
                    priorToken = token;
                    continue;
                }

                // TODO : avoid looking forward
                // if next in the line is something else than whitespace, than this is not the case
                if ((i + 1) < n)
                {
                    var nextToken = node.ScriptTokenStream[i + 1];
                    if (nextToken.Line == token.Line
                    && nextToken.TokenType != TSqlTokenType.WhiteSpace)
                    {
                        priorToken = token;
                        continue;
                    }
                }

                // if prior in the line is something else than whitespace, than this is not the case
                if ((i > 0)
                && (priorToken.Line == token.Line)
                && (priorToken.TokenType != TSqlTokenType.WhiteSpace))
                {
                    priorToken = token;
                    continue;
                }

                string script = token.Text;

                if (string.IsNullOrEmpty(script) || string.Equals(Environment.NewLine, script))
                {
                    priorToken = token;
                    continue;
                }

                // TODO : avoid splitting strings
                DetectWhitespaceInLines(
                    SplitRegex.Split(script),
                    token.Line);

                priorToken = token;
            }
        }

        private void DetectWhitespaceInLines(string[] lines, int startLine)
        {
            int lineOffset = 0;

            // linebreaks are treated as separate tokens thus starting with 0 and reflecting each line of spaces
            foreach (string line in lines)
            {
                if (line.Length > 0 && string.IsNullOrWhiteSpace(line))
                {
                    HandleLineError(startLine + lineOffset, 0);
                }

                lineOffset++;
            }
        }
    }
}
