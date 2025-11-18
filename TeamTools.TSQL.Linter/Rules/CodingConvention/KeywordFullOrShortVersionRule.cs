using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Keyword spelling validation.
    /// </summary>
    [RuleIdentity("CV0290", "KEYWORD_SHORTHAND")]
    internal sealed partial class KeywordFullOrShortVersionRule : AbstractRule
    {
        public KeywordFullOrShortVersionRule() : base()
        {
        }

        // TODO : try to collapse two very similar methods but avoid many delegates instantiation
        private static int LocateBadKeyword(KeywordWithShorthand keyword, int firstToken, int lastToken, IList<TSqlParserToken> tokenStream)
        {
            TSqlTokenType badToken = KeywordSpelling[keyword].Item1;

            for (int i = firstToken; i <= lastToken; i++)
            {
                var token = tokenStream[i];
                if (token.TokenType == TSqlTokenType.Semicolon)
                {
                    return -1;
                }
                else if (token.TokenType == badToken)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int LocateBadKeywordText(string badText, int firstToken, int lastToken, IList<TSqlParserToken> tokenStream)
        {
            for (int i = firstToken; i <= lastToken; i++)
            {
                var token = tokenStream[i];
                if (token.TokenType == TSqlTokenType.Semicolon)
                {
                    return -1;
                }
                else if (token.Text.Equals(badText, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private void ValidateSpelling(KeywordWithShorthand keyword, int firstToken, int lastToken, TSqlFragment node)
        {
            int badTokenIndex = keyword == KeywordWithShorthand.Output
                ? LocateBadKeywordText("OUT", firstToken, lastToken, node.ScriptTokenStream)
                : LocateBadKeyword(keyword, firstToken, lastToken, node.ScriptTokenStream);

            if (badTokenIndex == -1)
            {
                return;
            }

            HandleTokenError(node.ScriptTokenStream[badTokenIndex], KeywordSpelling[keyword].Item2);
        }
    }
}
