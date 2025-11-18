using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0208", "TWO_WORDS_JOIN")]
    internal sealed class JoinSpellingRule : AbstractRule, ICodeFixProvider
    {
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

                if (IsJoinKeywordToken(token.TokenType))
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

        private static bool IsJoinKeywordToken(TSqlTokenType token)
        {
            return token == TSqlTokenType.Join
                || token == TSqlTokenType.Right
                || token == TSqlTokenType.Left
                || token == TSqlTokenType.Inner
                || token == TSqlTokenType.Outer
                || token == TSqlTokenType.Full
                || token == TSqlTokenType.Cross;
        }
    }
}
