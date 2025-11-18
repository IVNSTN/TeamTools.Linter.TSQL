using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0192", "EXISTS_OUTPUT_CONTROL")]
    internal sealed class ExistsOutputControlRule : AbstractRule
    {
        public ExistsOutputControlRule() : base()
        {
        }

        public override void Visit(ExistsPredicate node)
        {
            var spec = node.Subquery.QueryExpression.GetQuerySpecification();
            if (spec is null)
            {
                return;
            }

            HandleNodeErrorIfAny(spec.OrderByClause);
            HandleNodeErrorIfAny(spec.TopRowFilter);
            HandleNodeErrorIfAny(spec.OffsetClause);

            if (spec.GroupByClause != null && spec.HavingClause is null)
            {
                HandleNodeError(spec.GroupByClause);
            }

            if (spec.UniqueRowFilter != UniqueRowFilter.NotSpecified)
            {
                HandleNodeError(spec);
            }

            // intersect/except need columns
            if (spec.SelectElements.Count > 1
            && !(node.Subquery.QueryExpression is BinaryQueryExpression bin && bin.BinaryQueryExpressionType != BinaryQueryExpressionType.Union))
            {
                HandleNodeError(spec.SelectElements[1]);
            }
        }
    }
}
