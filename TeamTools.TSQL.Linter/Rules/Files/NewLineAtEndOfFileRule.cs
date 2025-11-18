using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FL0306", "EOF_NEWLINE")]
    internal sealed class NewLineAtEndOfFileRule : AbstractRule
    {
        public NewLineAtEndOfFileRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            if (CountTrailingLines(node, out var lastToken) > 0)
            {
                return;
            }

            if (lastToken is null || lastToken.TokenType == TSqlTokenType.EndOfFile)
            {
                // empty file
                return;
            }

            HandleLineError(lastToken.Line, 1);
        }

        private static int CountTrailingLines(TSqlFragment node, out TSqlParserToken token)
        {
            int newLineCount = 0;
            token = default;

            for (int i = node.LastTokenIndex; i >= 0; i--)
            {
                token = node.ScriptTokenStream[i];

                switch (token.TokenType)
                {
                    case TSqlTokenType.EndOfFile: break;
                    case TSqlTokenType.WhiteSpace:
                        {
                            // -1 because 1 means no line breaks
                            newLineCount += token.Text.LineCount() - 1;
                            if (newLineCount > 0)
                            {
                                // all good
                                return newLineCount;
                            }

                            break;
                        }

                    // did not find new line and already detected something else
                    default: return newLineCount;
                }
            }

            return newLineCount;
        }
    }
}
