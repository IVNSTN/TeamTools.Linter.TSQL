using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0208", "TWO_WORDS_JOIN")]
    internal sealed class JoinSpellingRule : AbstractRule, ICodeFixProvider
    {
        private static readonly IList<TSqlTokenType> JoinTokenTypes = new List<TSqlTokenType>
        {
            TSqlTokenType.Join,
            TSqlTokenType.Right,
            TSqlTokenType.Left,
            TSqlTokenType.Inner,
            TSqlTokenType.Outer,
            TSqlTokenType.Full,
            TSqlTokenType.Cross,
        };

        public JoinSpellingRule() : base()
        {
        }

        public override void Visit(JoinTableReference node)
        {
            if (CountKeywords(node.FirstTableReference, node.SecondTableReference, node is UnqualifiedJoin) != 2)
            {
                HandleNodeError(node);
            }
        }

        private static int CountKeywords(TableReference first, TableReference second, bool supportApply)
        {
            int keywords = 0;
            int start = first.LastTokenIndex + 1;
            int end = second.FirstTokenIndex;

            for (var i = start; i < end; i++)
            {
                var token = first.ScriptTokenStream[i];

                if (JoinTokenTypes.Contains(token.TokenType))
                {
                    keywords++;
                }
                else if (supportApply && token.TokenType == TSqlTokenType.Identifier
                && token.Text.Equals("APPLY", StringComparison.OrdinalIgnoreCase))
                {
                    keywords++;
                }
            }

            return keywords;
        }
    }
}
