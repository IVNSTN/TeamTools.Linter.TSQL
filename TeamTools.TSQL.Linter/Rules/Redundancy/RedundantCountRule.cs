using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;

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
            // attempt to locate comparison against literal
            // when literal is on right side
            IntegerLiteral literal = ExtractIntLiteral(node.SecondExpression);
            var valueSourceExpression = node.FirstExpression;
            var comparison = node.ComparisonType;

            if (literal is null)
            {
                // maybe literal is on left side
                literal = ExtractIntLiteral(node.FirstExpression);
                // comparison normalization
                comparison = MirrorComparison(comparison);
                valueSourceExpression = node.SecondExpression;
            }

            if (literal is null || !int.TryParse(literal.Value, out int literalValue))
            {
                // not a comparison against integer literal
                return;
            }

            var subquery = ExtractSubQuery(valueSourceExpression);

            if (subquery is null || !IsSimpleCountQuery(subquery))
            {
                // not a "SELECT COUNT(1) > 0" pattern
                return;
            }

            ValidateLiteralComparisonPredicate(node, literalValue, comparison);
        }

        private static BooleanComparisonType MirrorComparison(BooleanComparisonType cmp)
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

                // mirroring with normalization to supported operators
                case BooleanComparisonType.NotGreaterThan:
                    return BooleanComparisonType.GreaterThanOrEqualTo;

                case BooleanComparisonType.NotLessThan:
                    return BooleanComparisonType.LessThanOrEqualTo;

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
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            // TODO : why positive only?
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
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
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
                // this includes NULL literal
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
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is ScalarSubquery q)
            {
                return ExtractQuerySpecification(q.QueryExpression);
            }

            return default;
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

            return default;
        }

        private static IntegerLiteral ExtractIntLiteral(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is IntegerLiteral i)
            {
                return i;
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

                return default;
            }

            return default;
        }

        private static string GetViolationMessage(int literalValue, BooleanComparisonType comparison)
        {
            if ((comparison == BooleanComparisonType.GreaterThan && literalValue == 0)
            || (comparison == BooleanComparisonType.GreaterThanOrEqualTo && literalValue == 1))
            {
                // COUNT(1) > 0, COUNT(1) >= 1
                return Strings.ViolationDetails_RedundantCountRule_UseExists;
            }
            else if ((comparison == BooleanComparisonType.LessThan && literalValue == 1)
            || (comparison == BooleanComparisonType.Equals && literalValue == 0)
            // count cannot be negative
            || (comparison == BooleanComparisonType.LessThanOrEqualTo && literalValue == 0))
            {
                // COUNT (1) < 1, COUNT(1) == 0, COUNT(1) <= 0
                return Strings.ViolationDetails_RedundantCountRule_UseNotExists;
            }
            else if (comparison == BooleanComparisonType.GreaterThanOrEqualTo && literalValue == 0)
            {
                // COUNT(1) >= 0
                // if there are no rows to count, count will still return zero
                // thus >=0 is "always true"
                return Strings.ViolationDetails_RedundantCountRule_PredicateIsTrue;
            }
            else if (comparison == BooleanComparisonType.LessThan && literalValue == 0)
            {
                // COUNT(1) < 0
                // if there are no rows to count, count will still return zero
                // thus < 0 is "always false"
                return Strings.ViolationDetails_RedundantCountRule_PredicateIsFalse;
            }

            // comparison to exact count or something not supported
            return default;
        }

        private void ValidateLiteralComparisonPredicate(TSqlFragment node, int literalValue, BooleanComparisonType comparison)
        {
            string details = GetViolationMessage(literalValue, comparison);

            if (string.IsNullOrEmpty(details))
            {
                return;
            }

            HandleNodeError(node, details);
        }
    }
}
