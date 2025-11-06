using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class SemicolonBeforeStatementFinder
    {
        public static void FindSemicolon(TSqlFragment node, Action callback)
        {
            bool foundSemicolon = false;
            int i = node.FirstTokenIndex - 1;

            while (i >= 0 && !foundSemicolon)
            {
                switch (node.ScriptTokenStream[i].TokenType)
                {
                    case TSqlTokenType.Semicolon:
                        foundSemicolon = true;
                        break;
                    case TSqlTokenType.WhiteSpace:
                    case TSqlTokenType.SingleLineComment:
                    case TSqlTokenType.MultilineComment:
                        // ignoring
                        break;
                    default:
                        // something else means there is no semicolon
                        i = -1;
                        break;
                }

                i--;
            }

            if (foundSemicolon)
            {
                return;
            }

            callback.Invoke();
        }
    }
}
