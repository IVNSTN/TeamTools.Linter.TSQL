using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0952", "VIEW_SORTED_OUTPUT")]
    internal sealed class SortedOutputFromViewRule : AbstractRule
    {
        public SortedOutputFromViewRule() : base()
        {
        }

        public override void Visit(ViewStatementBody node)
        {
            if (node.SelectStatement.QueryExpression.OrderByClause is null)
            {
                // A view should not have TOP limit or FETCH-OFFSET part
                // thus cannot be sorted. External query should apply sort order it is needing.
                return;
            }

            HandleNodeError(node.SelectStatement.QueryExpression.OrderByClause);
        }
    }
}
