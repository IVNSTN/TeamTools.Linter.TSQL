using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0720", "SORTED_CTE")]
    internal sealed class SortedOutputFromCteRule : AbstractRule
    {
        private static readonly int TopPercentLimit = 50;
        private static readonly int TopCountLimit = 10000;

        public SortedOutputFromCteRule() : base()
        {
        }

        public override void Visit(CommonTableExpression node)
        {
            var qs = node.QueryExpression.GetQuerySpecification();

            if (qs is null || qs.OrderByClause is null)
            {
                return;
            }

            if (qs.ForClause != null || qs.OffsetClause != null)
            {
                // order by is legal
                return;
            }

            if (IsValidTopLimit(qs.TopRowFilter))
            {
                return;
            }

            HandleNodeError(qs.OrderByClause);
        }

        private static bool IsValidTopLimit(TopRowFilter top)
        {
            if (top is null)
            {
                return false;
            }

            if (!(top.Expression is IntegerLiteral topValue
            && int.TryParse(topValue.Value, out int topIntValue)))
            {
                // unclear top definition
                return true;
            }

            return (top.Percent && topIntValue < TopPercentLimit)
                || ((!top.Percent) && topIntValue < TopCountLimit);
        }
    }
}
