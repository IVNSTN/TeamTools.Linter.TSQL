using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0278", "ALIAS_WITH_AS")]
    internal sealed class AliasWithAsSyntaxRule : AbstractRule
    {
        public AliasWithAsSyntaxRule() : base()
        {
        }

        public override void Visit(TableReferenceWithAlias node)
        {
            if (node.Alias is null)
            {
                return;
            }

            if (!IsValidAliasWithAsBefore(node.Alias))
            {
                HandleNodeError(node.Alias, node.Alias.Value);
            }
        }

        public override void Visit(QuerySpecification node)
        {
            int n = node.SelectElements.Count;
            for (int i = 0; i < n; i++)
            {
                if (node.SelectElements[i] is SelectScalarExpression col
                && col.ColumnName != null && col.ColumnName.Identifier != null)
                {
                    if (!IsValidAliasWithAsBefore(col.ColumnName.Identifier, col.Expression))
                    {
                        HandleNodeError(col.ColumnName, col.ColumnName.Value);
                    }
                }
            }
        }

        private static bool IsValidAliasWithAsBefore(Identifier colName, TSqlFragment colExpression = null)
        {
            int startLimit = 0;
            if (colExpression != null)
            {
                startLimit = colExpression.LastTokenIndex;

                // deprecated 'alias = expression' syntax is also outlaw
                if (colExpression.FirstTokenIndex > colName.FirstTokenIndex)
                {
                    return false;
                }
            }

            int i = colName.FirstTokenIndex - 1;

            while (i > startLimit && ScriptDomExtension.IsSkippableTokens(colName.ScriptTokenStream[i].TokenType))
            {
                i--;
            }

            return i > 0 && colName.ScriptTokenStream[i].TokenType == TSqlTokenType.As;
        }
    }
}
