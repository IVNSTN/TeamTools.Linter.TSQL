using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.TypeHandling;

namespace TeamTools.TSQL.Linter.Routines.ExpressionEvaluator.Evaluation
{
    // TODO : refactoring needed
    // TODO : cover with tests after refactoring
    [ExcludeFromCodeCoverage]
    public class VariableConditionalLimitDetector : IConditionalFlowHandler
    {
        private readonly IVariableEvaluatedValueRegistry varRegistry;
        private readonly IVariableEvaluator varEvaluator;
        private readonly IExpressionEvaluator exprEvaluator;
        private readonly IViolationRegistrar violations;

        public VariableConditionalLimitDetector(
            IVariableEvaluatedValueRegistry varRegistry,
            IVariableEvaluator varEvaluator,
            IExpressionEvaluator exprEvaluator,
            IViolationRegistrar violations)
        {
            this.varRegistry = varRegistry ?? throw new ArgumentNullException(nameof(varRegistry));
            this.varEvaluator = varEvaluator ?? throw new ArgumentNullException(nameof(varEvaluator));
            this.exprEvaluator = exprEvaluator ?? throw new ArgumentNullException(nameof(exprEvaluator));
            this.violations = violations ?? throw new ArgumentNullException(nameof(violations));
        }

        // TODO : decompose method, split into strategies
        // TODO : support negation
        // TODO : not equals to either of int variable bounds should narrow the bounds
        public bool DetectPredicatesLimitingVarValues(BooleanExpression node)
        {
            while (node is BooleanParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            // TODO : support multiple OR as equivalent of IN, simple AND
            if (node is BooleanBinaryExpression bin && bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
            {
                var first = DetectPredicatesLimitingVarValues(bin.FirstExpression);
                var second = DetectPredicatesLimitingVarValues(bin.SecondExpression);
                return first || second;
            }

            if (node is BooleanIsNullExpression nl && !nl.IsNot)
            {
                var varName = DetectVariableReference(nl.Expression);
                if (string.IsNullOrEmpty(varName))
                {
                    return false;
                }

                varRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, SqlValueKind.Null, new SqlValueSource(SqlValueSourceKind.Expression, node));

                return true;
            }

            if (node is InPredicate inpr && !inpr.NotDefined)
            {
                string varName = DetectVariableReference(inpr.Expression);
                if (string.IsNullOrEmpty(varName))
                {
                    return false;
                }

                SqlValue ev = null;
                foreach (var v in inpr.Values)
                {
                    var e = exprEvaluator.EvaluateExpression(v);
                    if (ev != null)
                    {
                        ev = ev.GetTypeHandler().MergeTwoEstimates(ev, e);
                    }
                    else
                    {
                        ev = e;
                    }
                }

                if (ev is null)
                {
                    return false;
                }

                ev.Source = new SqlValueSource(SqlValueSourceKind.Expression, node);

                varRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, ev);
                return true;
            }

            // TODO : this hardcoded crutch supports strings only
            if (node is BooleanComparisonExpression cmp)
            {
                var functionCall = DetectLengthLimit(cmp.FirstExpression);
                var sizeNode = cmp.SecondExpression;
                // inequality does not seem to mean any predictable value
                var topSizeLimitComparisons = new List<BooleanComparisonType> { BooleanComparisonType.LessThan, BooleanComparisonType.LessThanOrEqualTo, BooleanComparisonType.Equals };

                if (string.IsNullOrEmpty(functionCall.FuncName))
                {
                    functionCall = DetectLengthLimit(cmp.SecondExpression);
                    sizeNode = cmp.FirstExpression;
                    // reversing comparison operators that can limit var size
                    topSizeLimitComparisons = new List<BooleanComparisonType> { BooleanComparisonType.GreaterThan, BooleanComparisonType.GreaterThanOrEqualTo, BooleanComparisonType.Equals };
                }

                if (!string.IsNullOrEmpty(functionCall.FuncName))
                {
                    if (string.IsNullOrEmpty(functionCall.VarName))
                    {
                        return false;
                    }

                    // text size is limited by length value
                    // FIXME : each evaluation here can lead to dup violation registration
                    var ev = exprEvaluator.EvaluateExpression(sizeNode);

                    if (ev is null || !(ev is SqlIntTypeValue intVal))
                    {
                        return false;
                    }

                    var varType = varEvaluator.GetVariableTypeReference(functionCall.VarName);

                    if (varType is null || !(varType is SqlStrTypeReference strRef))
                    {
                        return false;
                    }

                    int sizeLimit = intVal.IsPreciseValue ? intVal.Value : intVal.EstimatedSize.High;

                    if (sizeLimit >= strRef.Size)
                    {
                        // size limit is no better than no limit
                        return false;
                    }

                    // TODO : to factory/handler
                    ev = new SqlStrTypeValue(
                        varType.MakeNullValue().GetTypeHandler() as SqlStrTypeHandler,
                        new SqlStrTypeReference(varType.TypeName, sizeLimit, varType.MakeNullValue().GetTypeHandler()?.ValueFactory),
                        SqlValueKind.Unknown,
                        new SqlValueSource(SqlValueSourceKind.Expression, node));

                    // TODO : for DATALENGTH also support that N-chars are 2-byte long
                    if ((string.Equals(functionCall.FuncName, "LEN", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(functionCall.FuncName, "DATALENGTH", StringComparison.OrdinalIgnoreCase))
                    && topSizeLimitComparisons.Contains(cmp.ComparisonType))
                    {
                        varRegistry.RegisterEvaluatedValue(functionCall.VarName, node.LastTokenIndex, ev);
                        return true;
                    }
                }
                else if (cmp.ComparisonType == BooleanComparisonType.Equals)
                {
                    // not a function call but direct variable comparison
                    // <, > and so on are not supported for strings for now
                    SqlValue ev;
                    string varName = DetectVariableReference(cmp.FirstExpression);

                    if (string.IsNullOrEmpty(varName))
                    {
                        varName = DetectVariableReference(cmp.SecondExpression);
                        ev = exprEvaluator.EvaluateExpression(cmp.FirstExpression);
                    }
                    else
                    {
                        ev = exprEvaluator.EvaluateExpression(cmp.SecondExpression);
                    }

                    if (ev is null || string.IsNullOrEmpty(varName))
                    {
                        return false;
                    }

                    ev.Source = new SqlValueSource(SqlValueSourceKind.Expression, node);

                    varRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, ev);
                    return true;
                }
                else if (cmp.ComparisonType == BooleanComparisonType.NotEqualToBrackets
                || cmp.ComparisonType == BooleanComparisonType.NotEqualToExclamation)
                {
                    // narrowing int value range
                    SqlValue ev;
                    string varName = DetectVariableReference(cmp.FirstExpression);

                    if (string.IsNullOrEmpty(varName))
                    {
                        varName = DetectVariableReference(cmp.SecondExpression);
                        ev = exprEvaluator.EvaluateExpression(cmp.FirstExpression);
                    }
                    else
                    {
                        ev = exprEvaluator.EvaluateExpression(cmp.SecondExpression);
                    }

                    var typeRef = varEvaluator.GetVariableTypeReference(varName);

                    if (ev is null || string.IsNullOrEmpty(varName) || typeRef is null)
                    {
                        return false;
                    }

                    if (ev is SqlIntTypeValue intLimit && intLimit.IsPreciseValue
                    && typeRef is SqlIntTypeReference intRef)
                    {
                        SqlIntValueRange newRange;

                        if (intLimit.Value == intRef.Size.Low)
                        {
                            newRange = new SqlIntValueRange(intRef.Size.Low + 1, intRef.Size.High);
                        }
                        else if (intLimit.Value == intRef.Size.High)
                        {
                            newRange = new SqlIntValueRange(intRef.Size.Low, intRef.Size.High - 1);
                        }
                        else
                        {
                            return false;
                        }

                        varRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, intLimit.ChangeTo(newRange, new SqlValueSource(SqlValueSourceKind.Expression, node)));

                        return true;
                    }

                    return false;
                }
                else if (cmp.ComparisonType == BooleanComparisonType.GreaterThan
                || cmp.ComparisonType == BooleanComparisonType.GreaterThanOrEqualTo
                || cmp.ComparisonType == BooleanComparisonType.LessThan
                || cmp.ComparisonType == BooleanComparisonType.LessThanOrEqualTo)
                {
                    // narrowing int value range
                    SqlValue ev;
                    string varName = DetectVariableReference(cmp.FirstExpression);
                    bool varOnLeftSide = true;

                    if (string.IsNullOrEmpty(varName))
                    {
                        varName = DetectVariableReference(cmp.SecondExpression);
                        ev = exprEvaluator.EvaluateExpression(cmp.FirstExpression);
                        varOnLeftSide = false;
                    }
                    else
                    {
                        ev = exprEvaluator.EvaluateExpression(cmp.SecondExpression);
                    }

                    var typeRef = varEvaluator.GetVariableTypeReference(varName);

                    if (ev is null || string.IsNullOrEmpty(varName) || typeRef is null)
                    {
                        return false;
                    }

                    var varValue = varEvaluator.GetValueAt(varName, cmp.FirstTokenIndex);
                    if (varValue != null)
                    {
                        typeRef = varValue.TypeReference;
                    }

                    var comparison = cmp.ComparisonType;
                    if (!varOnLeftSide)
                    {
                        comparison = RevertComparison(comparison);
                    }

                    if (ev is SqlIntTypeValue intLimit && intLimit.IsPreciseValue
                    && typeRef is SqlIntTypeReference intRef)
                    {
                        SqlIntValueRange newRange;

                        if (comparison == BooleanComparisonType.GreaterThan)
                        {
                            newRange = new SqlIntValueRange(intLimit.Value + 1, intRef.Size.High);
                        }
                        else if (comparison == BooleanComparisonType.GreaterThanOrEqualTo)
                        {
                            newRange = new SqlIntValueRange(intLimit.Value, intRef.Size.High);
                        }
                        else if (comparison == BooleanComparisonType.LessThan)
                        {
                            newRange = new SqlIntValueRange(intRef.Size.Low, intLimit.Value - 1);
                        }
                        else if (comparison == BooleanComparisonType.LessThanOrEqualTo)
                        {
                            newRange = new SqlIntValueRange(intRef.Size.Low, intLimit.Value);
                        }
                        else
                        {
                            return false;
                        }

                        varRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, intLimit.ChangeTo(newRange, new SqlValueSource(SqlValueSourceKind.Expression, node)));

                        return true;
                    }
                }
            }

            return false;
        }

        public bool DetectEqualityLimitingVarValues(ScalarExpression sourceValue, ScalarExpression limitDefinition)
        {
            string varName = DetectVariableReference(sourceValue);
            if (string.IsNullOrEmpty(varName))
            {
                return false;
            }

            var ev = exprEvaluator.EvaluateExpression(limitDefinition);
            if (ev is null)
            {
                return false;
            }

            // If there are variables on both sides of equality predicate
            // then the shortest one should put a limit on the longer one, not elsewise.
            if (ev.Source is SqlValueSourceVariable rightVarSrc)
            {
                var leftVarValue = exprEvaluator.EvaluateExpression(sourceValue);
                var rightVarTypeRef = varEvaluator.GetVariableTypeReference(rightVarSrc.VarName);

                if (rightVarTypeRef != null && leftVarValue != null
                && string.Equals(rightVarTypeRef.TypeName, leftVarValue.TypeName)
                && leftVarValue.TypeReference.CompareTo(rightVarTypeRef) < 0)
                {
                    // if right var is bigger than the left one,
                    // then the left is a limit for ther right one
                    varName = rightVarSrc.VarName;
                    ev = leftVarValue;
                }
            }

            varRegistry.RegisterEvaluatedValue(varName, limitDefinition.LastTokenIndex, ev);

            return true;
        }

        public void ResetValueEstimatesAfterConditionalBlock(TSqlFragment node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            int lastToken = node.LastTokenIndex;
            if (node is VariableReference
            || (node is WhenClause w && w.ThenExpression is VariableReference))
            {
                // For example in CASE WHEN @var = 'a' THEN @var
                // value limit lasts till THEN clause end
                // and the very last token is the variable reference
                // if we reset on var reference token index
                // then we will not be able to use the evaluated value.
                lastToken += 1;
            }

            varRegistry.ResetEvaluatedValuesAfterBlock(node.FirstTokenIndex, lastToken, default);
        }

        public void RevertValueEstimatesToBeforeBlock(TSqlFragment node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            varRegistry.RevertValueEstimatesToBeforeBlock(node.FirstTokenIndex, node.LastTokenIndex);
        }

        // TODO : support more cases, not only straightforward var reference
        private static string DetectVariableReference(TSqlFragment node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is VariableReference vr)
            {
                return vr.Name;
            }

            return default;
        }

        // TODO : dive deeper to find var limiting conditions
        private static FuncOnVar DetectLengthLimit(TSqlFragment node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is FunctionCall fn && fn.Parameters.Count > 0)
            {
                string varName = DetectVariableReference(fn.Parameters[0]);

                if (!string.IsNullOrEmpty(varName))
                {
                    return new FuncOnVar { FuncName = fn.FunctionName.Value, VarName = varName };
                }
            }

            return default;
        }

        private static BooleanComparisonType RevertComparison(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.GreaterThan:
                    return BooleanComparisonType.LessThan;
                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return BooleanComparisonType.LessThanOrEqualTo;
                case BooleanComparisonType.LessThan:
                    return BooleanComparisonType.GreaterThan;
                case BooleanComparisonType.LessThanOrEqualTo:
                    return BooleanComparisonType.GreaterThanOrEqualTo;
                default:
                    return cmp;
            }
        }

        private struct FuncOnVar
        {
            public string FuncName;

            public string VarName;
        }
    }
}
