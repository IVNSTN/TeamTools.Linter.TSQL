using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0232", "TRAILING_SPACES")]
    internal sealed class TrailingSpacesRule : AbstractRule
    {
        public TrailingSpacesRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            string whitespace = "";
            int startingLine = -1;
            int startingToken = -1;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.WhiteSpace)
                {
                    whitespace += node.ScriptTokenStream[i].Text;
                    if (startingLine == -1)
                    {
                        startingLine = node.ScriptTokenStream[i].Line;
                        startingToken = i;
                    }
                }
                else if (startingLine >= 0)
                {
                    if (!string.IsNullOrEmpty(whitespace) && HasTrailingSpaces(whitespace, startingLine, out int badLine)
                    && startingToken > 0)
                    {
                        if (badLine == startingLine)
                        {
                            HandleTokenError(node.ScriptTokenStream[startingToken]);
                        }
                        else
                        {
                            // if there is 1 or more linebreaks without trailing spaces
                            // then we cannot report on the beginning of the whitespace token
                            HandleLineError(badLine, 0);
                        }
                    }

                    whitespace = "";
                    startingLine = -1;
                    startingToken = -1;
                }
            }
        }

        private static bool HasTrailingSpaces(string whitespace, int startingLine, out int badLine)
        {
            // TODO: use Regex, stop producing arrays of strings!
            // Replacements are needed to handle correctly single \r or \n symbols
            // which may occur in files not formatted correctly with \r\n line endings.
            string[] lines = whitespace
                .Replace(Environment.NewLine, "\n")
                .Replace('\r', '\n')
                .Split('\n');

            badLine = startingLine;

            if (lines.Length <= 1)
            {
                // if no linebreaks included assuming that whitespace does not reach line ending
                return false;
            }

            // the last line means there are no more linebreaks so there is something else
            // thus this can't be a trailing whitespace
            for (int j = 0; j < lines.Length - 1; j++)
            {
                if (lines[j].Length > 0)
                {
                    badLine = startingLine + j;

                    // one violation per whitespace
                    return true;
                }
            }

            return false;
        }
    }
}
