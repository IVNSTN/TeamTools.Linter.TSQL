using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0719", "SCALAR_PREDICATE_AS_EXISTS")]
    internal sealed class ScalarPredicateTurnedIntoExistsRule : AbstractRule
    {
        public ScalarPredicateTurnedIntoExistsRule() : base()
        {
        }

        public override void Visit(ExistsPredicate node)
        {
            if (!ValidateExistsQuery(node.Subquery.QueryExpression))
            {
                HandleNodeError(node);
            }
        }

        private static bool IsIntersectExcept(BinaryQueryExpressionType expressionType)
        {
            return expressionType == BinaryQueryExpressionType.Except
                || expressionType == BinaryQueryExpressionType.Intersect;
        }

        private static bool ValidateExistsQuery(QueryExpression query)
        {
            if (query is BinaryQueryExpression bin)
            {
                if (IsIntersectExcept(bin.BinaryQueryExpressionType))
                {
                    // INTERSECT and EXCEPT use both queries as sources thus EXISTS
                    // might have sence over this expression
                    return true;
                }

                // At least one of subqueries is enough to treat EXISTS as valid
                return ValidateExistsQuery(bin.FirstQueryExpression)
                    || ValidateExistsQuery(bin.SecondQueryExpression);
            }

            var qs = query.GetQuerySpecification();

            return qs is null || qs.FromClause != null;
        }
    }
}
