using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0309", "TAB_SYMBOL")]
    internal sealed class TabSymbolRule : AbstractRule
    {
        private static readonly int MaxViolationsPerFile = 3;

        public TabSymbolRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            int violationCount = 0;
            for (int i = node.ScriptTokenStream.Count - 1; i >= 0; i--)
            {
                var token = node.ScriptTokenStream[i];
                if (!string.IsNullOrEmpty(token.Text)
                && IsTokenWithText(token.TokenType))
                {
                    var tab = token.Text.IndexOf('\t');

                    if (tab >= 0)
                    {
                        // TODO : report on tab position precisely. note, whitespace may contain linebreaks.
                        // currently it breaks delivery to sonar
                        // +1 because index is zero-based
                        HandleLineError(token.Line, token.Column);
                        violationCount++;
                        if (violationCount >= MaxViolationsPerFile)
                        {
                            return;
                        }
                    }
                }
            }
        }

        private static bool IsTokenWithText(TSqlTokenType tokenType)
        {
            return tokenType == TSqlTokenType.WhiteSpace
                || tokenType == TSqlTokenType.SingleLineComment
                || tokenType == TSqlTokenType.MultilineComment
                || tokenType == TSqlTokenType.AsciiStringLiteral
                || tokenType == TSqlTokenType.UnicodeStringLiteral
                || tokenType == TSqlTokenType.QuotedIdentifier
                || tokenType == TSqlTokenType.AsciiStringOrQuotedIdentifier
                || tokenType == TSqlTokenType.Identifier;
        }
    }
}
