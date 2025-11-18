using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0159", "NON_ANSI_NULL_COMPARISON")]
    internal sealed class NonAnsiNullOperationRule : AbstractRule
    {
        public NonAnsiNullOperationRule() : base()
        {
        }

        public override void Visit(BinaryExpression node) => ValidateExpression(node.FirstExpression).ValidateExpression(node.SecondExpression);

        public override void Visit(BooleanComparisonExpression node) => ValidateExpression(node.FirstExpression).ValidateExpression(node.SecondExpression);

        public override void Visit(SimpleCaseExpression node) => ValidateExpression(node.InputExpression);

        public override void Visit(SimpleWhenClause node) => ValidateExpression(node.WhenExpression);

        public override void Visit(InPredicate node) => ValidateExpression(node.Expression).ValidateStatement(node.Values);

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
            else if (node is UnaryExpression un)
            {
                // + or - NULL
                return GetExpression(un.Expression);
            }

            return node;
        }

        private NonAnsiNullOperationRule ValidateExpression(ScalarExpression node)
        {
            if (GetExpression(node) is NullLiteral)
            {
                HandleNodeError(node);
            }

            return this;
        }

        private void ValidateStatement(IList<ScalarExpression> expressions)
        {
            int n = expressions.Count;
            for (int i = 0; i < n; i++)
            {
                ValidateExpression(expressions[i]);
            }
        }
    }
}
