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

            foreach (var col in node.OrderByElements)
            {
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
