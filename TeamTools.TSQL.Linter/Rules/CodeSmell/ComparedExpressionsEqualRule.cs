using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0853", "COMPARISON_LEFT_EQUALS_RIGHT")]
    internal sealed class ComparedExpressionsEqualRule : AbstractRule
    {
        public ComparedExpressionsEqualRule() : base()
        {
        }

        public override void Visit(BooleanComparisonExpression node)
            => ValidateComparisonSides(node.FirstExpression, node.SecondExpression);

        public override void Visit(LikePredicate node)
            => ValidateComparisonSides(node.FirstExpression, node.SecondExpression);

        public override void Visit(BooleanTernaryExpression node)
        {
            ValidateComparisonSides(node.FirstExpression, node.SecondExpression);
            ValidateComparisonSides(node.FirstExpression, node.ThirdExpression);
        }

        private static bool AreEqualExpressions(ScalarExpression left, ScalarExpression right)
        {
            while (left is ParenthesisExpression pe)
            {
                left = pe.Expression;
            }

            while (right is ParenthesisExpression pe)
            {
                right = pe.Expression;
            }

            if (left is Literal l && int.TryParse(l.Value, out int literalValue)
            && (literalValue == 0 || literalValue == 1))
            {
                // 1 = 1, 0 != 0 are used sometimes on purpose
                return false;
            }

            return BooleanExpressionComparer.AreEqualExpressions(left, right);
        }

        private void ValidateComparisonSides(ScalarExpression left, ScalarExpression right)
        {
            if (AreEqualExpressions(left, right))
            {
                HandleNodeError(right);
            }
        }
    }
}
