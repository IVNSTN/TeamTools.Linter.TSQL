using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0995", "FLOAT_WITH_NON_FLOAT")]
    internal sealed class FloatMixedWithNonFloatRule : AbstractRule
    {
        // TODO : support table columns
        public FloatMixedWithNonFloatRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var typeEvaluator = new ExpressionResultTypeEvaluator(node);
            var expressionValidator = new FloatMixValidator(typeEvaluator, ViolationHandlerWithMessage);
            node.Accept(expressionValidator);
        }

        private class FloatMixValidator : TSqlFragmentVisitor
        {
            private static readonly HashSet<string> FloatTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "FLOAT",
                "REAL",
            };

            private readonly ExpressionResultTypeEvaluator evaluator;
            private readonly Action<TSqlFragment, string> callback;

            public FloatMixValidator(ExpressionResultTypeEvaluator evaluator, Action<TSqlFragment, string> callback)
            {
                this.evaluator = evaluator;
                this.callback = callback;
            }

            public override void Visit(DeclareVariableElement node)
            {
                if (node.Value is null)
                {
                    return;
                }

                if (node.Value.IsMadeOfLiteral())
                {
                    // literals will be implicitly converted
                    return;
                }

                ValidateTypeCombination(node, node, node.Value);
            }

            public override void Visit(SelectSetVariable node)
            {
                if (node.Expression.IsMadeOfLiteral())
                {
                    // literals will be implicitly converted
                    return;
                }

                ValidateTypeCombination(node, node.Variable, node.Expression);
            }

            public override void Visit(SetVariableStatement node)
            {
                if (node.Expression.IsMadeOfLiteral())
                {
                    // literals will be implicitly converted
                    return;
                }

                ValidateTypeCombination(node, node.Variable, node.Expression);
            }

            public override void Visit(BinaryExpression node)
            {
                if (node.FirstExpression.IsMadeOfLiteral() || node.SecondExpression.IsMadeOfLiteral())
                {
                    // literals will be implicitly converted
                    return;
                }

                ValidateTypeCombination(node, node.FirstExpression, node.SecondExpression);
            }

            public override void Visit(BooleanComparisonExpression node)
            {
                if (node.FirstExpression.IsMadeOfLiteral() || node.SecondExpression.IsMadeOfLiteral())
                {
                    // literals will be implicitly converted
                    return;
                }

                ValidateTypeCombination(node, node.FirstExpression, node.SecondExpression);
            }

            private void ValidateTypeCombination(TSqlFragment node, DeclareVariableElement declaration, ScalarExpression initialization)
            {
                ValidateTypeCombination(
                    node,
                    evaluator.GetExpressionType(declaration),
                    evaluator.GetExpressionType(initialization));
            }

            private void ValidateTypeCombination(TSqlFragment node, ScalarExpression expressionA, ScalarExpression expressionB)
            {
                ValidateTypeCombination(
                    node,
                    evaluator.GetExpressionType(expressionA),
                    evaluator.GetExpressionType(expressionB));
            }

            // Implicit conversion possibility is not validated here -
            // separate rules should be responsible for this
            private void ValidateTypeCombination(TSqlFragment node, string typeA, string typeB)
            {
                if (string.IsNullOrEmpty(typeA) || string.IsNullOrEmpty(typeB))
                {
                    // both types must be revealed
                    return;
                }

                if (string.Equals(typeA, typeB, StringComparison.OrdinalIgnoreCase))
                {
                    // types are equal
                    return;
                }

                if (!(FloatTypes.Contains(typeA) || FloatTypes.Contains(typeB)))
                {
                    // neither is float
                    return;
                }

                if (FloatTypes.Contains(typeA) && FloatTypes.Contains(typeB))
                {
                    // both are float or real
                    return;
                }

                callback?.Invoke(node, string.Format("{0} and {1}", typeA, typeB));
            }
        }
    }
}
