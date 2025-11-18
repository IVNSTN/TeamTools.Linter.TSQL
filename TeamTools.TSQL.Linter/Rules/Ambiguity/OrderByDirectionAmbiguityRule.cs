using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0154", "ORDERBY_DIRECTION_AMBIGUOUS")]
    internal sealed class OrderByDirectionAmbiguityRule : AbstractRule
    {
        public OrderByDirectionAmbiguityRule() : base()
        {
        }

        public override void Visit(OrderByClause node)
        {
            bool allDirectionsSpecified = true;
            bool descExists = false;

            for (int i = 0, n = node.OrderByElements.Count; i < n; i++)
            {
                var col = node.OrderByElements[i];
                allDirectionsSpecified &= col.SortOrder != SortOrder.NotSpecified;
                descExists |= col.SortOrder == SortOrder.Descending;
            }

            if (allDirectionsSpecified || !descExists)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
