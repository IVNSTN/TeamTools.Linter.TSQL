using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0248", "REDUNDANT_SEMICOLON")]
    internal sealed class RedundantSemicolonRule : AbstractRule
    {
        public RedundantSemicolonRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            int semicolonCounter = 0;
            int lastSemicolonTokenIndex = -1;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.Semicolon)
                {
                    semicolonCounter++;
                    lastSemicolonTokenIndex = i;
                }
                else if (node.ScriptTokenStream[i].TokenType == TSqlTokenType.WhiteSpace)
                {
                    // keep counter value
                }
                else
                {
                    if (semicolonCounter > 1)
                    {
                        HandleTokenError(node.ScriptTokenStream[lastSemicolonTokenIndex]);
                    }

                    semicolonCounter = 0;
                }
            }

            // if this was the very last token
            if (semicolonCounter > 1)
            {
                HandleTokenError(node.ScriptTokenStream[lastSemicolonTokenIndex]);
            }
        }
    }
}
