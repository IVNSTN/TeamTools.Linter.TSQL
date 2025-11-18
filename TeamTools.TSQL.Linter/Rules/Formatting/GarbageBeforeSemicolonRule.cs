using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0715", "SEMICOLON_IS_THE_END")]
    // TODO : A little similar to SemicolonAfterEachStatementRule
    internal sealed class GarbageBeforeSemicolonRule : AbstractRule
    {
        public GarbageBeforeSemicolonRule() : base()
        {
        }

        public override void Visit(TSqlStatement node)
        {
            var start = node.FirstTokenIndex;
            var i = node.LastTokenIndex;
            var semicolonToken = node.ScriptTokenStream[i];

            if (TSqlTokenType.Semicolon != semicolonToken.TokenType)
            {
                // no semicolon
                return;
            }

            // looking for the very first ending semicolon in the statement
            // redundant multiple semicolons, dangling semicolons
            // are controlled by a separate rule
            for (; i > start; i--)
            {
                var token = node.ScriptTokenStream[i];

                if (TSqlTokenType.Semicolon != token.TokenType
                && !ScriptDomExtension.IsSkippableTokens(token.TokenType))
                {
                    break;
                }

                if (token.TokenType == TSqlTokenType.Semicolon)
                {
                    semicolonToken = token;
                }
            }

            // it will be either semicolon or garbage
            var firstTokenAfterCode = node.ScriptTokenStream[i + 1];

            if (i > start && ScriptDomExtension.IsSkippableTokens(firstTokenAfterCode.TokenType))
            {
                HandleLineError(semicolonToken.Line, semicolonToken.Column, firstTokenAfterCode.TokenType.ToString());
            }
        }
    }
}
