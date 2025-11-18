using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0944", "EXISTS_INSTEAD_OF_COUNT")]
    internal sealed class RedundantCountRule : AbstractRule
    {
        public RedundantCountRule() : base()
        {
        }

        public override void Visit(BooleanComparisonExpression node)
        {
            IntegerLiteral literal = ExtractIntLiteral(node.SecondExpression);
            var valueSourceExpression = node.FirstExpression;
            var comparison = node.ComparisonType;

            if (literal is null)
            {
                literal = ExtractIntLiteral(node.FirstExpression);
                comparison = RevertComparison(comparison);
                valueSourceExpression = node.SecondExpression;
            }

            if (literal is null || !int.TryParse(literal.Value, out int literalValue))
            {
                return;
            }

            var subquery = ExtractSubQuery(valueSourceExpression);

            if (subquery is null || !IsSimpleCountQuery(subquery))
            {
                return;
            }

            ValidateLiteralComparisonPredicate(node, literalValue, comparison);
        }

        private static BooleanComparisonType RevertComparison(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.GreaterThan:
                    return BooleanComparisonType.LessThan;

                case BooleanComparisonType.LessThan:
                    return BooleanComparisonType.GreaterThan;

                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return BooleanComparisonType.LessThanOrEqualTo;

                case BooleanComparisonType.LessThanOrEqualTo:
                    return BooleanComparisonType.GreaterThanOrEqualTo;

                default:
                    return cmp;
            }
        }

        private static bool IsSimpleCountQuery(QuerySpecification query)
        {
            if (query.SelectElements.Count != 1)
            {
                return false;
            }

            if (!(query.SelectElements[0] is SelectScalarExpression sel))
            {
                return false;
            }

            return IsSelectCountExpression(sel.Expression);
        }

        private static bool IsSelectCountExpression(ScalarExpression node)
        {
            if (node is ParenthesisExpression pe)
            {
                return IsSelectCountExpression(pe.Expression);
            }

            if (node is UnaryExpression un && un.UnaryExpressionType == UnaryExpressionType.Positive)
            {
                return IsSelectCountExpression(un.Expression);
            }

            if (node is FunctionCall fn && fn.FunctionName.Value.Equals("COUNT", StringComparison.OrdinalIgnoreCase))
            {
                if (fn.Parameters.Count != 1)
                {
                    return false;
                }

                return IsImmutableValue(fn.Parameters[0]);
            }

            return false;
        }

        private static bool IsImmutableValue(ScalarExpression node)
        {
            if (node is ParenthesisExpression pe)
            {
                return IsImmutableValue(pe.Expression);
            }

            if (node is BinaryExpression bin)
            {
                // e.g. 1+1
                return IsImmutableValue(bin.FirstExpression)
                    && IsImmutableValue(bin.SecondExpression);
            }

            if (node is UnaryExpression un)
            {
                // e.g. +1
                return IsImmutableValue(un.Expression);
            }

            if (node is Literal)
            {
                return true;
            }

            if (node is ColumnReferenceExpression col && col.ColumnType == ColumnType.Wildcard)
            {
                // COUNT(*)
                return true;
            }

            return false;
        }

        private static QuerySpecification ExtractSubQuery(ScalarExpression node)
        {
            if (node is ParenthesisExpression pe)
            {
                return ExtractSubQuery(pe.Expression);
            }

            if (node is ScalarSubquery q)
            {
                return ExtractQuerySpecification(q.QueryExpression);
            }

            return null;
        }

        private static QuerySpecification ExtractQuerySpecification(QueryExpression q)
        {
            if (q is BinaryQueryExpression bin)
            {
                return ExtractQuerySpecification(bin.FirstQueryExpression);
            }

            if (q is QueryParenthesisExpression qp)
            {
                return ExtractQuerySpecification(qp.QueryExpression);
            }

            if (q is QuerySpecification spec && spec.FromClause != null)
            {
                return spec;
            }

            return null;
        }

        private static IntegerLiteral ExtractIntLiteral(ScalarExpression node)
        {
            if (node is IntegerLiteral i)
            {
                return i;
            }

            if (node is ParenthesisExpression pe)
            {
                return ExtractIntLiteral(pe.Expression);
            }

            if (node is UnaryExpression un && un.UnaryExpressionType == UnaryExpressionType.Positive)
            {
                // TODO : -1 != +1 support sign reversion
                return ExtractIntLiteral(un.Expression);
            }

            if (node is ScalarSubquery q && q.QueryExpression is QuerySpecification spec)
            {
                if (spec.FromClause is null && spec.SelectElements.Count == 1
                && spec.SelectElements[0] is SelectScalarExpression sel)
                {
                    return ExtractIntLiteral(sel.Expression);
                }

                return null;
            }

            return null;
        }

        private void ValidateLiteralComparisonPredicate(TSqlFragment node, int literalValue, BooleanComparisonType comparison)
        {
            string details;

            if ((comparison == BooleanComparisonType.GreaterThan && literalValue == 0)
            || (comparison == BooleanComparisonType.GreaterThanOrEqualTo && literalValue == 1))
            {
                details = "use EXISTS";
            }
            else if ((comparison == BooleanComparisonType.LessThan && literalValue == 1)
            || (comparison == BooleanComparisonType.Equals && literalValue == 0)
            // count cannot be negative
            || (comparison == BooleanComparisonType.LessThanOrEqualTo && literalValue == 0))
            {
                details = "use NOT EXISTS";
            }
            else if (comparison == BooleanComparisonType.GreaterThanOrEqualTo && literalValue == 0)
            {
                // if there are no rows to count, count will still return zero
                // thus >=0 is "always true"
                details = "predicate is always false";
            }
            else if (comparison == BooleanComparisonType.LessThan && literalValue == 0)
            {
                // if there are no rows to count, count will still return zero
                // thus < 0 is "always false"
                details = "predicate is always false";
            }
            else
            {
                // comparison to exact count or something not supported
                return;
            }

            HandleNodeError(node, details);
        }
    }
}
