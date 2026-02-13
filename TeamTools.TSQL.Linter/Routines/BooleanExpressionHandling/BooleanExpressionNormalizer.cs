using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class BooleanExpressionNormalizer
    {
        // Normalizing boolean predicates to be able to detect equal expression even
        // if they are written in reversed or negated way
        // e.g. (@a < @b) is the same as (@b > @a) and NOT (@a >= @b)
        public static BooleanExpressionParts Normalize(BooleanExpression expr)
        {
            var result = new BooleanExpressionParts
            {
                OriginalExpression = expr,
            };

            // TODO : BooleanBinaryExpression? at least sort
            if (expr is BooleanComparisonExpression cmp)
            {
                // TODO : handle NotGreaterThan as negation
                result.ComparisonType = GetNormalizedComparisonType(cmp.ComparisonType);
                result.FirstExpression = ExtractExpression(cmp.FirstExpression);
                result.SecondExpression = ExtractExpression(cmp.SecondExpression);

                if (result.ComparisonType != cmp.ComparisonType)
                {
                    // switching sides of expression if needed for normalization
                    (result.FirstExpression, result.SecondExpression) = (result.SecondExpression, result.FirstExpression);
                }
                else if (result.ComparisonType == BooleanComparisonType.Equals)
                {
                    // normalizing by sorting alphabetically if comparison has no direction (< or >)
                    string firstExpression = result.FirstExpression.GetFragmentCleanedText();
                    string secondExpression = result.SecondExpression.GetFragmentCleanedText();
                    if (string.Compare(firstExpression, secondExpression) > 0)
                    {
                        // switching sides of expression if needed for normalization
                        (result.FirstExpression, result.SecondExpression) = (result.SecondExpression, result.FirstExpression);
                    }
                }
                else if (result.ComparisonType == BooleanComparisonType.NotEqualToExclamation)
                {
                    // replacing with equal comparison type for similarity
                    // !=  ->  <>
                    result.ComparisonType = BooleanComparisonType.NotEqualToBrackets;
                }
            }
            else if (expr is BooleanIsNullExpression isnull)
            {
                result.ComparisonType = isnull.IsNot ? BooleanComparisonType.NotEqualToBrackets : BooleanComparisonType.Equals;
                result.FirstExpression = ExtractExpression(isnull.Expression);
                result.SecondExpression = null;
            }
            else if (expr is BooleanNotExpression not)
            {
                // expanding NOT (@a >= @b) to @a < @b and normalizing to @b > @a
                result = Normalize(ExtractExpression(not.Expression));
                var comparisonType = GetNormalizedComparisonType(BooleanComparisonConverter.RevertComparison(result.ComparisonType));
                if (comparisonType != result.ComparisonType)
                {
                    result.ComparisonType = comparisonType;
                    if (result.SecondExpression != null)
                    {
                        // switching sides of expression if needed for normalization
                        // unless this is IS NULL / IS NOT NULL expression
                        (result.FirstExpression, result.SecondExpression) = (result.SecondExpression, result.FirstExpression);
                    }
                }
            }

            return result;
        }

        private static ScalarExpression ExtractExpression(ScalarExpression expr)
            => BooleanExpressionPartsExtractor.ExtractExpression(expr);

        private static BooleanExpression ExtractExpression(BooleanExpression expr)
            => BooleanExpressionPartsExtractor.ExtractExpression(expr);

        // Normalizing comparison types: all left-sided (<, <=, !<) to right-sided (>, >=, !>)
        private static BooleanComparisonType GetNormalizedComparisonType(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.LessThan:
                    return BooleanComparisonType.GreaterThan;

                case BooleanComparisonType.LessThanOrEqualTo:
                    return BooleanComparisonType.GreaterThanOrEqualTo;

                case BooleanComparisonType.NotLessThan:
                    return BooleanComparisonType.NotGreaterThan;

                default:
                    return cmp;
            }
        }
    }
}
