using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0715", "SEMICOLON_IS_THE_END")]
    // TODO : A little similar to SemicolonAfterEachStatementRule
    internal sealed class GarbageBeforeSemicolonRule : AbstractRule
    {
        private static readonly ICollection<TSqlTokenType> GarbageTokens;

        static GarbageBeforeSemicolonRule()
        {
            GarbageTokens = new List<TSqlTokenType>
            {
                TSqlTokenType.WhiteSpace,
                TSqlTokenType.SingleLineComment,
                TSqlTokenType.MultilineComment,
            };
        }

        public GarbageBeforeSemicolonRule() : base()
        {
        }

        public override void Visit(TSqlStatement node)
        {
            var start = node.FirstTokenIndex;
            var i = node.LastTokenIndex;

            if (TSqlTokenType.Semicolon != node.ScriptTokenStream[i].TokenType)
            {
                // no semicolon
                return;
            }

            var semicolonToken = node.ScriptTokenStream[i];

            // looking for the very first ending semicolon in the statement
            // redundant multiple semicolons, dangling semicolons
            // are controlled by a separate rule
            while (i > start && (TSqlTokenType.Semicolon == node.ScriptTokenStream[i].TokenType
            || GarbageTokens.Contains(node.ScriptTokenStream[i].TokenType)))
            {
                if (TSqlTokenType.Semicolon == node.ScriptTokenStream[i].TokenType)
                {
                    semicolonToken = node.ScriptTokenStream[i];
                }

                i--;
            }

            // it will be either semicolon or garbage
            var firstTokenAfterCode = node.ScriptTokenStream[i + 1];

            if (i > start && GarbageTokens.Contains(firstTokenAfterCode.TokenType))
            {
                HandleLineError(semicolonToken.Line, semicolonToken.Column, firstTokenAfterCode.TokenType.ToString());
            }
        }
    }
}
