using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0808", "DISTINCT_FOR_IN")]
    internal sealed class DistinctForInSubqueryRule : AbstractRule
    {
        public DistinctForInSubqueryRule() : base()
        {
        }

        public override void Visit(InPredicate node)
        {
            if (node.Subquery is null)
            {
                return;
            }

            var qs = node.Subquery.QueryExpression.GetQuerySpecification();
            if (qs is null)
            {
                return;
            }

            if (qs.UniqueRowFilter.IsDistinct())
            {
                HandleNodeError((TSqlFragment)qs.SelectElements.FirstOrDefault() ?? qs);
            }
        }
    }
}
