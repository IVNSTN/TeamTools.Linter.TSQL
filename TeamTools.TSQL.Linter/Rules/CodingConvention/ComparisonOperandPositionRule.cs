using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0467", "COMPARISON_OPERAND_POSITION")]
    internal sealed class ComparisonOperandPositionRule : AbstractRule
    {
        public ComparisonOperandPositionRule() : base()
        {
        }

        public override void Visit(BooleanComparisonExpression node)
        {
            var leftSide = ExpandExpression(node.FirstExpression);
            bool leftIsVariableOrLiteral = IsVariableOrLiteral(leftSide);

            if (!leftIsVariableOrLiteral)
            {
                return;
            }

            var rightSide = ExpandExpression(node.SecondExpression);
            bool rightIsVariableOrLiteral = IsVariableOrLiteral(rightSide);

            if (rightIsVariableOrLiteral)
            {
                // literal/variable are on both sides - literal must be on right side
                if (rightSide is Literal || leftSide is VariableReference)
                {
                    return;
                }
            }

            HandleNodeError(node.FirstExpression);
        }

        private static bool IsVariableOrLiteral(TSqlFragment node)
        {
            return node is VariableReference
                || node is Literal;
        }

        private static TSqlFragment ExpandExpression(TSqlFragment node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is UnaryExpression un)
            {
                return ExpandExpression(un.Expression);
            }

            return node;
        }
    }
}
