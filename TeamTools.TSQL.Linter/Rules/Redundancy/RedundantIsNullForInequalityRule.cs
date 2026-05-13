using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0883", "REDUNDANT_ISNULL_NOT_EQUALS")]
    internal sealed class RedundantIsNullForInequalityRule : AbstractRule
    {
        private static readonly string IsNullFunction = "ISNULL";

        public RedundantIsNullForInequalityRule() : base()
        {
        }

        public override void Visit(BooleanComparisonExpression node)
        {
            if (node.ComparisonType == BooleanComparisonType.Equals
            || node.ComparisonType == BooleanComparisonType.GreaterThanOrEqualTo
            || node.ComparisonType == BooleanComparisonType.LessThanOrEqualTo
            || node.ComparisonType == BooleanComparisonType.NotLessThan
            || node.ComparisonType == BooleanComparisonType.NotGreaterThan
            || node.ComparisonType == BooleanComparisonType.LeftOuterJoin
            || node.ComparisonType == BooleanComparisonType.RightOuterJoin)
            {
                // Possible equality is not our case
                return;
            }

            var left = node.FirstExpression.ExtractScalarExpression();
            var right = node.SecondExpression.ExtractScalarExpression();

            if (left is FunctionCall)
            {
                (left, right) = (right, left);
            }

            if (!(right is FunctionCall isnull
            && isnull.FunctionName.Value.Equals(IsNullFunction, StringComparison.OrdinalIgnoreCase)
            && isnull.Parameters.Count == 2))
            {
                // No ISNULL on the right side of comparison
                return;
            }

            if (BooleanExpressionComparer.AreEqualExpressions(left, isnull.Parameters[1].ExtractScalarExpression()))
            {
                // x <> ISNULL(y, x)
                HandleNodeError(isnull);
            }
        }
    }
}
