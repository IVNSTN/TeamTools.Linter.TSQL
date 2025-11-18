using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0224", "TRAN_CONTROL_TWO_WORDS")]
    internal sealed class TransactionControlInTwoWordsRule : AbstractRule
    {
        private static readonly Lazy<HashSet<TSqlTokenType>> TranControlTokensInstance
            = new Lazy<HashSet<TSqlTokenType>>(() => InitTranControlTokens(), true);

        public TransactionControlInTwoWordsRule() : base()
        {
        }

        private static HashSet<TSqlTokenType> TranControlTokens => TranControlTokensInstance.Value;

        public override void Visit(TransactionStatement node)
        {
            int keywordCount = 0;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex + 1;

            for (int i = start; i < end; i++)
            {
                if (TranControlTokens.Contains(node.ScriptTokenStream[i].TokenType))
                {
                    keywordCount++;
                }
            }

            if (keywordCount >= 2)
            {
                return;
            }

            HandleNodeError(node);
        }

        private static HashSet<TSqlTokenType> InitTranControlTokens()
        {
            return new HashSet<TSqlTokenType>
            {
                { TSqlTokenType.Begin },
                { TSqlTokenType.Commit },
                { TSqlTokenType.Rollback },
                { TSqlTokenType.Save },
                { TSqlTokenType.Tran },
                { TSqlTokenType.Transaction },
            };
        }
    }
}
