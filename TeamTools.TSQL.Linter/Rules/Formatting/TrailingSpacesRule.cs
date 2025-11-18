using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0232", "TRAILING_SPACES")]
    internal sealed class TrailingSpacesRule : AbstractRule
    {
        public TrailingSpacesRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.WhiteSpace)
                {
                    ValidateWhitespace(token, i == end ? null : node.ScriptTokenStream[i + 1]);
                }
            }
        }

        private void ValidateWhitespace(TSqlParserToken whiteSpaceToken, TSqlParserToken nextToken)
        {
            // WhiteSpace tokens contain spaces and linebreaks altogether
            const char carriageReturn = '\r';
            const char lineFeed = '\n';

            string whitespace = whiteSpaceToken.Text;
            if (string.IsNullOrEmpty(whitespace))
            {
                return;
            }

            bool treatAsLastLine = nextToken is null || nextToken.TokenType == TSqlTokenType.EndOfFile;
            if (!treatAsLastLine && nextToken != null
            && nextToken.TokenType == TSqlTokenType.WhiteSpace
            && !string.IsNullOrEmpty(nextToken.Text))
            {
                var c = nextToken.Text[0];
                if (c == carriageReturn || c == lineFeed)
                {
                    treatAsLastLine = true;
                }
            }

            bool lineBreakDetected = false;
            bool whiteSpaceDetected = false;
            int reportedLine = whiteSpaceToken.Line;
            int reportedCol = whiteSpaceToken.Column;

            int n = whitespace.Length;
            for (int i = 0; i < n; i++)
            {
                var c = whitespace[i];
                if (c == lineFeed)
                {
                    lineBreakDetected = true;
                    reportedLine++;
                }
                else if (c == carriageReturn)
                {
                    lineBreakDetected = true;
                    reportedLine++;

                    if (i < (n - 1) && whitespace[i + 1] == lineFeed)
                    {
                        // \r\n is a single line break
                        i++;
                    }
                }
                else if (char.IsWhiteSpace(c))
                {
                    // TODO : is there a reason to "double-check" for whitespace at all?
                    whiteSpaceDetected = true;
                    reportedCol++;
                }

                if (lineBreakDetected && whiteSpaceDetected)
                {
                    HandleLineError(reportedLine - 1, reportedCol - 1);

                    lineBreakDetected = false;
                    whiteSpaceDetected = false;
                    reportedCol = 0;
                }
            }

            if (whiteSpaceDetected && treatAsLastLine)
            {
                HandleLineError(reportedLine, reportedCol > 0 ? reportedCol - 1 : 0);
            }
        }
    }
}
