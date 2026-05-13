using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal abstract class BaseNoEqualityFilterRule : AbstractRule
    {
        protected BaseNoEqualityFilterRule() : base()
        {
        }

        // TODO : shouldn't it verify that a ColumnReferenceExpression is on the left side?
        protected static bool HasEqualityFilter(BooleanExpression node)
        {
            if (node is BooleanParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is BooleanBinaryExpression bin)
            {
                if (bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
                {
                    return HasEqualityFilter(bin.FirstExpression)
                        || HasEqualityFilter(bin.SecondExpression);
                }
                else
                {
                    return HasEqualityFilter(bin.FirstExpression)
                        && HasEqualityFilter(bin.SecondExpression);
                }
            }

            if (node is BooleanComparisonExpression cmp)
            {
                // 1 = 1 is no good
                return cmp.ComparisonType == BooleanComparisonType.Equals
                    && ExpressionsDiffer(cmp.FirstExpression, cmp.SecondExpression);
            }

            if (node is InPredicate inpred)
            {
                // NOT IN does not limit much
                return !inpred.NotDefined
                    && !inpred.Expression.IsMadeOfLiteral();
            }

            // LIKE can lead to INDEX SEEK
            if (node is LikePredicate like)
            {
                return ExpressionsDiffer(like.FirstExpression, like.SecondExpression);
            }

            // BETWEEN does not mean equality but limits range from both sides
            if (node is BooleanTernaryExpression between)
            {
                return ExpressionsDiffer(between.FirstExpression, between.SecondExpression)
                    && ExpressionsDiffer(between.FirstExpression, between.ThirdExpression);
            }

            if (node is BooleanIsNullExpression isnull)
            {
                // IS NOT NULL means any other value
                // however IS NULL seems to bee good enough
                return !isnull.IsNot
                    && !isnull.Expression.IsMadeOfLiteral();
            }

            if (node is BooleanNotExpression not)
            {
                // TODO : not sure if this negation + recursion is correct for any expression
                // return !HasEqualityFilter(not.Expression);
                return false;
            }

            if (node is ExistsPredicate exists)
            {
                // EXISTS can be treated as some kind of INNER JOIN with equality predicate
                return true;
            }

            return false;
        }

        protected void ValidatePredicate(BooleanExpression node)
        {
            if (HasEqualityFilter(node))
            {
                return;
            }

            HandleNodeError(node);
        }

        private static bool ExpressionsDiffer(ScalarExpression first, ScalarExpression second)
        {
            return !BooleanExpressionComparer.AreEqualExpressions(first, second);
        }
    }
}
