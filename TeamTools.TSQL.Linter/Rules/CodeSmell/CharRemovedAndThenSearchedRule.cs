using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0840", "CHAR_REMOVED_AND_SEARCHED")]
    internal sealed class CharRemovedAndThenSearchedRule : ScriptAnalysisServiceConsumingRule
    {
        public CharRemovedAndThenSearchedRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            if (!ScalarExpressionEvaluator.IsBatchInteresting(node))
            {
                return;
            }

            var expressionEvaluator = GetService<ScalarExpressionEvaluator>(node);

            node.Accept(new FuncCallVisitor(expressionEvaluator, ViolationHandler));
        }

        private sealed class FuncCallVisitor : VisitorWithCallback
        {
            private readonly ScalarExpressionEvaluator evaluator;

            public FuncCallVisitor(ScalarExpressionEvaluator evaluator, Action<TSqlFragment> callback) : base(callback)
            {
                this.evaluator = evaluator;
            }

            public override void Visit(BooleanComparisonExpression node)
            {
                if (!(node.ComparisonType == BooleanComparisonType.Equals
                || node.ComparisonType == BooleanComparisonType.NotEqualToBrackets
                || node.ComparisonType == BooleanComparisonType.NotEqualToExclamation))
                {
                    // > and < for strings seems to be fine even if replacement "conflicts" with compared value
                    return;
                }

                ExtractExpressionParts(node.FirstExpression.ExtractScalarExpression(), node.SecondExpression.ExtractScalarExpression(), out var comparedValue, out var functionCall);

                if (string.IsNullOrEmpty(comparedValue) || functionCall is null)
                {
                    return;
                }

                if ((functionCall.Parameters?.Count ?? 0) == 0)
                {
                    // TRIM and REPLACE need parameters
                    return;
                }

                string funcName = functionCall.FunctionName.Value;

                if (funcName.Equals("REPLACE", StringComparison.OrdinalIgnoreCase)
                && functionCall.Parameters.Count == 3)
                {
                    ValidateReplaceAndCompare(functionCall.Parameters[1], ExtractScalarValue(functionCall.Parameters[1]), comparedValue);
                }
                else if (funcName.EndsWith("TRIM", StringComparison.OrdinalIgnoreCase))
                {
                    bool trimStart = funcName.Equals("LTRIM", StringComparison.OrdinalIgnoreCase);
                    bool trimEnd = funcName.Equals("RTRIM", StringComparison.OrdinalIgnoreCase);

                    if (funcName.Equals("TRIM", StringComparison.OrdinalIgnoreCase))
                    {
                        if (functionCall.Parameters.Count == 1)
                        {
                            // no specific trim chars provided
                            ValidateTrimAndCompare(functionCall.FunctionName, " ", comparedValue, true, true);
                        }
                        else
                        {
                            // For TRIM FROM trimmed chars go first
                            ValidateTrimAndCompare(functionCall.Parameters[0], ExtractScalarValue(functionCall.Parameters[0]), comparedValue, true, true);
                        }
                    }
                    else if (trimStart || trimEnd)
                    {
                        if (functionCall.Parameters.Count == 1)
                        {
                            // no specific trim chars provided
                            ValidateTrimAndCompare(functionCall.Parameters[0], " ", comparedValue, trimStart, trimEnd);
                        }
                        else
                        {
                            // For LTRIM, RTRIM trimmed chars go last
                            ValidateTrimAndCompare(functionCall.Parameters[1], ExtractScalarValue(functionCall.Parameters[1]), comparedValue, trimStart, trimEnd);
                        }
                    }
                }
            }

            private void ValidateReplaceAndCompare(TSqlFragment node, string replacedSubstring, string comparedValue)
            {
                if (string.IsNullOrEmpty(replacedSubstring) || node is null)
                {
                    return;
                }

                if (comparedValue.Contains(replacedSubstring))
                {
                    Callback(node);
                }
            }

            private void ValidateTrimAndCompare(TSqlFragment node, string replacedSubstring, string comparedValue, bool trimStart, bool trimEnd)
            {
                if (string.IsNullOrEmpty(replacedSubstring) || node is null)
                {
                    return;
                }

                if ((trimStart && comparedValue.StartsWith(replacedSubstring))
                || (trimEnd && comparedValue.EndsWith(replacedSubstring)))
                {
                    Callback(node);
                }
            }

            private void ExtractExpressionParts(ScalarExpression leftSide, ScalarExpression rightSide, out string comparedValue, out FunctionCall functionCall)
            {
                comparedValue = null;
                functionCall = null;

                if (leftSide is null || rightSide is null)
                {
                    return;
                }

                if (rightSide is FunctionCall fn)
                {
                    functionCall = fn;
                    rightSide = leftSide;
                }
                else if (leftSide is FunctionCall ff)
                {
                    functionCall = ff;
                }
                else
                {
                    // Not a possible REPLACE/TRIM result comparison schenario
                    return;
                }

                comparedValue = ExtractScalarValue(rightSide);
            }

            private string ExtractScalarValue(ScalarExpression src)
            {
                if (src is StringLiteral s)
                {
                    return s.Value;
                }
                else if (src is VariableReference v)
                {
                    var evaluatedValue = evaluator.GetValueAt(v.Name, v.FirstTokenIndex);
                    if (evaluatedValue != null && evaluatedValue.IsPreciseValue && evaluatedValue is SqlStrTypeValue ss)
                    {
                        return ss.Value;
                    }
                }

                // no precise value
                return null;
            }
        }
    }
}
