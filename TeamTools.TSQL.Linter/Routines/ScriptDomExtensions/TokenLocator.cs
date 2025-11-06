using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class TokenLocator
    {
        public static TSqlParserToken LocateFirstBeforeOrDefault(this TSqlFragment node, TSqlTokenType token)
        {
            int i = node.FirstTokenIndex;
            while (i > 0)
            {
                if (node.ScriptTokenStream[i].TokenType == token)
                {
                    return node.ScriptTokenStream[i];
                }

                i--;
            }

            // default is the first token of the node
            return node.ScriptTokenStream[node.FirstTokenIndex];
        }
    }
}
