using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0275", "AS_WITH_SPACES")]
    internal sealed class SingleSpaceAroundAsRule : AbstractRule
    {
        public SingleSpaceAroundAsRule() : base()
        {
        }

        public override void Visit(TableReferenceWithAlias node) => ValidateAsSpaces(node.Alias);

        public override void Visit(SelectScalarExpression node)
        {
            // no alias or 'alias = expression' syntax used
            if (node.ColumnName is null
            || node.ColumnName.FirstTokenIndex < node.Expression.FirstTokenIndex)
            {
                return;
            }

            ValidateAsSpaces(node.ColumnName);
        }

        private static TSqlParserToken GetInvalidAsFormat(TSqlFragment node)
        {
            TSqlParserToken asKeyword = null;
            bool spacesAfterOk = true;

            // Scanning back from identifier to find 'AS' and check whitespace around it
            int i = node.FirstTokenIndex - 1;

            while (i > 0 && (ScriptDomExtension.IsSkippableTokens(node.ScriptTokenStream[i].TokenType)
                || node.ScriptTokenStream[i].TokenType == TSqlTokenType.As))
            {
                var token = node.ScriptTokenStream[i];
                if (token.TokenType == TSqlTokenType.As)
                {
                    asKeyword = node.ScriptTokenStream[i];
                    if (!spacesAfterOk)
                    {
                        return asKeyword;
                    }
                }
                else if (token.TokenType != TSqlTokenType.WhiteSpace
                || token.Text.Length != 1 || token.Text[0] != ' ')
                {
                    // if this is not whitespace at all or too long whitespace
                    // then we have to either remember that something is wrong
                    // and report it after we detect 'AS', or report immediately if we already met 'AS'
                    if (asKeyword is null)
                    {
                        spacesAfterOk = false;
                    }
                    else
                    {
                        return asKeyword;
                    }
                }

                i--;
            }

            // if we have scrolled till found some code and did not detect
            // any violation (or 'AS' keyword itself) then not reporting anything
            return default;
        }

        private void ValidateAsSpaces(TSqlFragment node)
        {
            if (node is null)
            {
                return;
            }

            var badAs = GetInvalidAsFormat(node);
            if (badAs is null)
            {
                return;
            }

            HandleTokenError(badAs);
        }
    }
}
