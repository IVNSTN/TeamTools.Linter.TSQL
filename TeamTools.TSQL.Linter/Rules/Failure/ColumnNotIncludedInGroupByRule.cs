using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : no GROUP BY but Aggregate function used -> this is the same case actually just no columns are grouped
    [RuleIdentity("FA0949", "COLUMN_NOT_IN_GROUP_BY")]
    internal sealed partial class ColumnNotIncludedInGroupByRule : AbstractRule
    {
        public ColumnNotIncludedInGroupByRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if (node.GroupByClause is null)
            {
                return;
            }

            var groupedExpressions = ExtractGroupedExpressions(node.GroupByClause.GroupingSpecifications);
            if (groupedExpressions.Count > 0)
            {
                ValidateSelectedExpressions(node.SelectElements, groupedExpressions);
            }
        }

        private void ValidateSelectedExpressions(IList<SelectElement> selected, HashSet<string> groupedExpressions)
        {
            var colVisitor = new ColumnReferenceVisitor(nonColumnIdentifiers);

            int n = selected.Count;
            for (int i = 0; i < n; i++)
            {
                var col = selected[i];
                if (!(col is SelectScalarExpression selExpr))
                {
                    // not interested in vars ans literals
                    continue;
                }

                if (!ContainsColumnReference(selExpr.Expression, colVisitor))
                {
                    continue;
                }

                if (!IsInvalidInSelect(selExpr.Expression, groupedExpressions))
                {
                    continue;
                }

                string selText = GetExpressionDefinitionText(selExpr.Expression);
                if (groupedExpressions.Contains(selText))
                {
                    // same expression exists in GROUP BY clause
                    continue;
                }

                // TODO : if there is only one column in the whole expression
                // which is the issue then report this column name only
                HandleNodeError(col, selText);
            }
        }
    }
}
