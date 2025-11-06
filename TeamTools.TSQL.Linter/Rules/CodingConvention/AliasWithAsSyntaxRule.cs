using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0278", "ALIAS_WITH_AS")]
    internal sealed class AliasWithAsSyntaxRule : AbstractRule
    {
        private static readonly IList<TSqlTokenType> SeparatorTokens = new List<TSqlTokenType>
        {
            TSqlTokenType.WhiteSpace,
            TSqlTokenType.SingleLineComment,
            TSqlTokenType.MultilineComment,
        };

        public AliasWithAsSyntaxRule() : base()
        {
        }

        public override void Visit(TableReferenceWithAlias node)
        {
            if (node.Alias != null && !ValidateAliasHasAsBefore(node.Alias))
            {
                HandleNodeError(node.Alias, node.Alias.Value);
            }
        }

        public override void Visit(QuerySpecification node)
        {
            var columnsWithBadAliases = node.SelectElements
                .OfType<SelectScalarExpression>()
                .Where(col => col.ColumnName != null && col.ColumnName.Identifier != null)
                .Where(col => col.Expression.FirstTokenIndex > col.ColumnName.FirstTokenIndex // alias = expression syntax
                    || !ValidateAliasHasAsBefore(col.ColumnName.Identifier, col.Expression.LastTokenIndex));

            foreach (var col in columnsWithBadAliases)
            {
                HandleNodeError(col.ColumnName, col.ColumnName.Value);
            }
        }

        private static bool ValidateAliasHasAsBefore(Identifier colName, int startLimit = 0)
        {
            int i = colName.FirstTokenIndex - 1;

            while (i > startLimit && SeparatorTokens.Contains(colName.ScriptTokenStream[i].TokenType))
            {
                i--;
            }

            return i > 0 && colName.ScriptTokenStream[i].TokenType == TSqlTokenType.As;
        }
    }
}
