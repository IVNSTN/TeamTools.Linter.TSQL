using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("SI0455", "EXTRACT_EXPRESSION")]
    internal sealed partial class ExtractExpressionRule : AbstractRule
    {
        public ExtractExpressionRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            var visitor = new ComplexExpressionVisitor(ViolationHandler);

            for (int i = 0, n = node.SelectElements.Count; i < n; i++)
            {
                node.SelectElements[i].Accept(visitor);
            }

            // Detecting expression repeats in other clauses
            node.FromClause?.Accept(visitor);
            node.GroupByClause?.Accept(visitor);
            node.WhereClause?.Accept(visitor);
        }
    }
}
