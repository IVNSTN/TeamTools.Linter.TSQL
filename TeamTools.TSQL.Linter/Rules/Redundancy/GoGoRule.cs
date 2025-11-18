using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0228", "GO_GO")]
    internal sealed class GoGoRule : AbstractRule
    {
        public GoGoRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            if (node.Batches.Count == 0)
            {
                return;
            }

            // GO-GO can be before the first batch
            CountGo(0, node.Batches[0].FirstTokenIndex, node.ScriptTokenStream);

            // Counting GO between batches
            int n = node.Batches.Count - 1;
            for (int i = 0; i < n; i++)
            {
                CountGo(node.Batches[i].LastTokenIndex + 1, node.Batches[i + 1].FirstTokenIndex, node.ScriptTokenStream);
            }

            // GO-GO can be after the last batch
            CountGo(node.Batches[n].LastTokenIndex + 1, node.ScriptTokenStream.Count, node.ScriptTokenStream);
        }

        private void CountGo(int start, int end, IList<TSqlParserToken> tokens)
        {
            int goCounter = 0;

            for (int i = start; i < end; i++)
            {
                var token = tokens[i];
                if (token.TokenType == TSqlTokenType.Go)
                {
                    goCounter++;
                }
                else if (!ScriptDomExtension.IsNonStatementToken(token.TokenType))
                {
                    goCounter = 0;
                }

                if (goCounter > 1)
                {
                    goCounter = 1;
                    HandleTokenError(token);
                }
            }
        }
    }
}
