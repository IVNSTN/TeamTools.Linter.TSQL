using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0152", "GROUPBY_DISTINCT")]
    internal sealed class DistinctAfterGroupByRule : AbstractRule
    {
        public DistinctAfterGroupByRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if (node.GroupByClause != null && node.UniqueRowFilter.IsDistinct())
            {
                HandleNodeError(node);
            }
        }
    }
}
