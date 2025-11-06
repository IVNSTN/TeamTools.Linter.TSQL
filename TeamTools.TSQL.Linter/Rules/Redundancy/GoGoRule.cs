using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0228", "GO_GO")]
    internal sealed class GoGoRule : AbstractRule
    {
        private static readonly Lazy<IList<TSqlTokenType>> IgnoredTokenTypesInstance
            = new Lazy<IList<TSqlTokenType>>(() => InitIgnoredTokenTypesInstance(), true);

        public GoGoRule() : base()
        {
        }

        private static IList<TSqlTokenType> IgnoredTokenTypes => IgnoredTokenTypesInstance.Value;

        public override void Visit(TSqlScript node)
        {
            int goCounter = 0;
            int start = node.FirstTokenIndex;
            int end = node.LastTokenIndex;

            for (int i = start; i <= end; i++)
            {
                var tokenType = node.ScriptTokenStream[i].TokenType;
                if (tokenType == TSqlTokenType.Go)
                {
                    goCounter += 1;
                }
                else if (!IgnoredTokenTypes.Contains(tokenType))
                {
                    goCounter = 0;
                }

                if (goCounter > 1)
                {
                    goCounter = 1;
                    HandleTokenError(node.ScriptTokenStream[i]);
                }
            }
        }

        private static IList<TSqlTokenType> InitIgnoredTokenTypesInstance()
        {
            return new List<TSqlTokenType>
            {
                TSqlTokenType.Semicolon,
                TSqlTokenType.WhiteSpace,
                TSqlTokenType.MultilineComment,
                TSqlTokenType.SingleLineComment,
            };
        }
    }
}
