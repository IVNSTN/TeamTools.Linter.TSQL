using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0159", "NON_ANSI_NULL_COMPARISON")]
    internal sealed class NonAnsiNullOperationRule : AbstractRule
    {
        public NonAnsiNullOperationRule() : base()
        {
        }

        public override void Visit(BinaryExpression node) => ValidateStatement(node.FirstExpression).ValidateStatement(node.SecondExpression);

        public override void Visit(BooleanComparisonExpression node) => ValidateStatement(node.FirstExpression).ValidateStatement(node.SecondExpression);

        public override void Visit(SimpleCaseExpression node) => ValidateStatement(node.InputExpression);

        public override void Visit(SimpleWhenClause node) => ValidateStatement(node.WhenExpression);

        private static TSqlFragment GetExpression(TSqlFragment node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is ScalarSubquery sue)
            {
                return GetExpression(sue.QueryExpression);
            }
            else if (node is QuerySpecification qs && qs.SelectElements.Count == 1)
            {
                return GetExpression(qs.SelectElements[0]);
            }
            else if (node is SelectScalarExpression se)
            {
                return GetExpression(se.Expression);
            }

            return node;
        }

        private NonAnsiNullOperationRule ValidateStatement(ScalarExpression node)
        {
            if (GetExpression(node) is NullLiteral)
            {
                HandleNodeError(node);
            }

            return this;
        }
    }
}
