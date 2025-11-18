using Microsoft.SqlServer.TransactSql.ScriptDom;
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

        // This is very similar to NewLineAtEndOfFileRule
        protected override void ValidateScript(TSqlScript node)
        {
            if (CountTrailingLines(node, out var lastToken) <= 1)
            {
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
                            break;
                        }

                    // found something other than whitespace
                    default: return newLineCount;
                }
            }

            return newLineCount;
        }
    }
}
