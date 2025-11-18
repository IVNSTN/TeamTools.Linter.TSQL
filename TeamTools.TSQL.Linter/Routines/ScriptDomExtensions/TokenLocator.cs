using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class TokenLocator
    {
        public static TSqlParserToken LocateFirstBeforeOrDefault(this TSqlFragment node, TSqlTokenType tokenType)
        {
            for (int i = node.FirstTokenIndex; i > 0; i--)
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == tokenType)
                {
                    return token;
                }
            }

            // default is the first token of the node
            return node.ScriptTokenStream[node.FirstTokenIndex];
        }

        public static bool LookForwardFor(this TSqlFragment node, TSqlTokenType tokenType, out int location)
            => node.LookForwardFor(node.FirstTokenIndex, node.LastTokenIndex, tokenType, out location);

        public static bool LookForwardFor(this TSqlFragment node, int start, int end, TSqlTokenType tokenType, out int location)
            => LookupForToken(node.ScriptTokenStream, start, end, +1, nd => nd.TokenType == tokenType, out location);

        public static bool LookForwardFor(this TSqlFragment node, int start, int end, string tokenValue, out int location)
            => LookupForToken(node.ScriptTokenStream, start, end, +1, nd => nd.Text.Equals(tokenValue, StringComparison.OrdinalIgnoreCase), out location);

        public static bool LookBackwardFor(this TSqlFragment node, int end, int start, TSqlTokenType tokenType, out int location)
            => LookupForToken(node.ScriptTokenStream, end, start, -1, nd => nd.TokenType == tokenType, out location);

        public static bool LookBackwardFor(this TSqlFragment node, int end, int start, string tokenValue, out int location)
            => LookupForToken(node.ScriptTokenStream, end, start, -1, nd => nd.Text.Equals(tokenValue, StringComparison.OrdinalIgnoreCase), out location);

        private static bool LookupForToken(IList<TSqlParserToken> stream, int start, int end, int step, Func<TSqlParserToken, bool> predicate, out int location)
        {
            if (start == -1 || end == -1)
            {
                Debug.Fail("bad input index value");

                location = default;
                return false;
            }

            while (start != end && !predicate.Invoke(stream[start]))
            {
                start += step;
            }

            if (start != end && predicate.Invoke(stream[start]))
            {
                location = start;
                return true;
            }

            location = default;
            return false;
        }
    }
}
