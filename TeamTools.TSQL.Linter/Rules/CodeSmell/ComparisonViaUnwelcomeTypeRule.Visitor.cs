using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.ExpressionEvaluator;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ComparisonViaUnwelcomeTypeRule
    {
        private sealed class ImplicitConvertionResultTypeValidator : TSqlFragmentVisitor
        {
            private static readonly HashSet<string> BadTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SQL_VARIANT",
                "FLOAT",
                "MONEY",
                "SMALLMONEY",
            };

            private readonly ExpressionResultTypeEvaluator evaluator;
            private readonly Action<TSqlFragment, string> callback;

            public ImplicitConvertionResultTypeValidator(ExpressionResultTypeEvaluator evaluator, Action<TSqlFragment, string> callback)
            {
                this.evaluator = evaluator;
                this.callback = callback;
            }

            // SearchedCaseExpression's WHEN clauses are visited by BooleanComparisonExpression visitor
            public override void Visit(SimpleCaseExpression node)
            {
                var comparisonType = evaluator.GetExpressionType(node.InputExpression);

                ValidateResultType(comparisonType, node.InputExpression);

                for (int i = node.WhenClauses.Count - 1; i >= 0; i--)
                {
                    var expr = node.WhenClauses[i].WhenExpression;
                    var whenType = evaluator.GetExpressionType(expr);

                    ValidateResultType(whenType, expr);
                }
            }

            public override void Visit(InPredicate node)
            {
                string comparisonType = node.Values != null
                    ? evaluator.GetExpressionType(node.Values)
                    : evaluator.GetExpressionType(node.Subquery);

                comparisonType = evaluator.GetResultingTypeOf(comparisonType, evaluator.GetExpressionType(node.Expression));

                ValidateResultType(comparisonType, node);
            }

            public override void Visit(BooleanComparisonExpression node)
            {
                var leftType = evaluator.GetExpressionType(node.FirstExpression);

                if (string.IsNullOrEmpty(leftType))
                {
                    // unable to determine
                    return;
                }

                var rightType = evaluator.GetExpressionType(node.SecondExpression);

                if (string.IsNullOrEmpty(rightType))
                {
                    // unable to determine
                    return;
                }

                ValidateResultType(node, leftType, rightType);
            }

            private void ValidateResultType(TSqlFragment node, string leftType, string rightType)
            {
                // Comparison will occur on nearest common data type for both types
                string comparisonType = evaluator.GetResultingTypeOf(leftType, rightType);

                ValidateResultType(comparisonType, node);
            }

            private void ValidateResultType(string typeName, TSqlFragment node)
            {
                if (string.IsNullOrEmpty(typeName))
                {
                    // unable to estimate type of expression
                    return;
                }

                if (!BadTypes.Contains(typeName))
                {
                    // valid type
                    return;
                }

                callback(node, typeName);
            }
        }
    }
}
