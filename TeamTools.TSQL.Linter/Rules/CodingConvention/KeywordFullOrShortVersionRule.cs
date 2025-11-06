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

        private static int LocateBadKeyword(KeywordWithShorthand keyword, int firstToken, int lastToken, IList<TSqlParserToken> tokenStream)
        {
            bool codeFixed = false;
            int i = firstToken;
            TSqlTokenType badToken = KeywordSpelling[keyword].Key;
            // some magic about OUTPUT case
            string badText = keyword == KeywordWithShorthand.Output ? "OUT" : null;

            while (i <= lastToken && !codeFixed && tokenStream[i].TokenType != TSqlTokenType.Semicolon)
            {
                // catching bad version
                if ((badText == null && tokenStream[i].TokenType == badToken)
                || (badText != null && tokenStream[i].Text.Equals(badText, StringComparison.OrdinalIgnoreCase)))
                {
                    codeFixed = true;
                }
                else
                {
                    i++;
                }
            }

            return codeFixed ? i : -1;
        }

        private void ValidateSpelling(KeywordWithShorthand keyword, int firstToken, int lastToken, TSqlFragment node)
        {
            int badTokenIndex = LocateBadKeyword(keyword, firstToken, lastToken, node.ScriptTokenStream);

            if (badTokenIndex == -1)
            {
                return;
            }

            HandleTokenError(node.ScriptTokenStream[badTokenIndex], KeywordSpelling[keyword].Value);
        }
    }
}
