using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TeamTools.TSQL.ExpressionEvaluator.Interfaces;
using TeamTools.TSQL.ExpressionEvaluator.TypeHandling;
using TeamTools.TSQL.ExpressionEvaluator.Values;

namespace TeamTools.TSQL.ExpressionEvaluator.Evaluation
{
    // TODO : refactoring needed
    // TODO : cover with tests after refactoring
    [ExcludeFromCodeCoverage]
    public class VariableConditionalLimitDetector : IConditionalFlowHandler
    {
        private static readonly List<BooleanComparisonType> NegatedStringSizeLimitingConditions = new List<BooleanComparisonType>
        {
            BooleanComparisonType.GreaterThan,
            BooleanComparisonType.GreaterThanOrEqualTo,
            BooleanComparisonType.NotEqualToBrackets,
            BooleanComparisonType.NotEqualToExclamation,
        };

        private static readonly List<BooleanComparisonType> NormalStringSizeLimitingConditions = new List<BooleanComparisonType>
        {
            BooleanComparisonType.LessThan,
            BooleanComparisonType.LessThanOrEqualTo,
            BooleanComparisonType.Equals,
        };

        private static readonly List<BooleanComparisonType> InversedNormalStringSizeLimitingConditions = new List<BooleanComparisonType>
        {
            BooleanComparisonType.GreaterThan,
            BooleanComparisonType.GreaterThanOrEqualTo,
            BooleanComparisonType.Equals,
        };

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

        public bool DetectPredicatesLimitingVarValues(BooleanExpression node) => DetectPredicatesLimitingVarValues(node, false);

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
                lastToken++;
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

        private static BooleanComparisonType NegateComparison(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.GreaterThan:
                    return BooleanComparisonType.LessThanOrEqualTo;
                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return BooleanComparisonType.LessThan;
                case BooleanComparisonType.LessThan:
                    return BooleanComparisonType.GreaterThanOrEqualTo;
                case BooleanComparisonType.LessThanOrEqualTo:
                    return BooleanComparisonType.GreaterThan;
                case BooleanComparisonType.Equals:
                    return BooleanComparisonType.NotEqualToBrackets;
                case BooleanComparisonType.NotEqualToBrackets:
                    return BooleanComparisonType.Equals;
                case BooleanComparisonType.NotEqualToExclamation:
                    return BooleanComparisonType.Equals;
                case BooleanComparisonType.NotGreaterThan:
                    return BooleanComparisonType.LessThanOrEqualTo;
                case BooleanComparisonType.NotLessThan:
                    return BooleanComparisonType.GreaterThanOrEqualTo;
                default:
                    return cmp;
            }
        }

        private static BooleanComparisonType NegateComparisonIfNeeded(BooleanComparisonType cmp, bool negated)
        {
            if (negated)
            {
                return NegateComparison(cmp);
            }

            return cmp;
        }

        // TODO : decompose method, split into strategies
        private bool DetectPredicatesLimitingVarValues(BooleanExpression node, bool negated)
        {
            while (node is BooleanParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is BooleanNotExpression bnot)
            {
                return DetectPredicatesLimitingVarValues(bnot.Expression, !negated);
            }

            // TODO : support multiple OR as equivalent of IN, simple AND
            if (node is BooleanBinaryExpression bin
            && ((!negated && bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
                || (negated && bin.BinaryExpressionType == BooleanBinaryExpressionType.Or)))
            {
                var first = DetectPredicatesLimitingVarValues(bin.FirstExpression, negated);
                var second = DetectPredicatesLimitingVarValues(bin.SecondExpression, negated);
                return first || second;
            }

            if (node is BooleanIsNullExpression nl)
            {
                var varName = DetectVariableReference(nl.Expression);
                if (string.IsNullOrEmpty(varName))
                {
                    return false;
                }

                // both true -> not-not -> without negation, both false - no negation
                if (negated == nl.IsNot)
                {
                    // var is NULL
                    varRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, SqlValueKind.Null, new SqlValueSource(SqlValueSourceKind.Expression, node));
                }
                else
                {
                    // var is NOT NULL
                    varRegistry.RegisterEvaluatedValue(varName, node.LastTokenIndex, SqlValueKind.Unknown, new SqlValueSource(SqlValueSourceKind.Expression, node));
                }

                return true;
            }

            // both true -> not-not -> without negation, both false - no negation
            if (node is InPredicate inpr && negated == inpr.NotDefined)
            {
                string varName = DetectVariableReference(inpr.Expression);
                if (string.IsNullOrEmpty(varName))
                {
                    return false;
                }

                SqlValue ev = null;
                int n = inpr.Values.Count;
                for (int i = 0; i < n; i++)
                {
                    var v = inpr.Values[i];

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

            if (node is BooleanComparisonExpression cmp)
            {
                var comparison = NegateComparisonIfNeeded(cmp.ComparisonType, negated);
                var functionCall = DetectLengthLimit(cmp.FirstExpression);

                var sizeNode = cmp.SecondExpression;
                // TODO : this hardcoded crutch supports strings only
                var topSizeLimitComparisons =
                    negated
                    ? NegatedStringSizeLimitingConditions
                    // If not negated then inequality does not seem to mean any predictable value
                    : NormalStringSizeLimitingConditions;

                // Left part of comparision is not a LEN or DATALENGTH call
                if (string.IsNullOrEmpty(functionCall.FuncName))
                {
                    functionCall = DetectLengthLimit(cmp.SecondExpression);
                    sizeNode = cmp.FirstExpression;
                    // reversing comparison operators that can limit var size
                    // TODO : this does not seem to respect ternary operator choosing
                    // list of comparisons several lines above...
                    topSizeLimitComparisons = InversedNormalStringSizeLimitingConditions;
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
                else if (comparison == BooleanComparisonType.Equals)
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
                else if (comparison == BooleanComparisonType.NotEqualToBrackets || comparison == BooleanComparisonType.NotEqualToExclamation)
                {
                    // narrowing int value range
                    SqlValue ev;
                    string varName = DetectVariableReference(cmp.FirstExpression);

                    if (string.IsNullOrEmpty(varName))
                    {
                        // Looking for variable reference on right side if not found on left side of comparison
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

                        // If inequality limit matches low or high type limit, then we can
                        // narrow down the awailable value range by 1
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
                else if (comparison == BooleanComparisonType.GreaterThan
                || comparison == BooleanComparisonType.GreaterThanOrEqualTo
                || comparison == BooleanComparisonType.LessThan
                || comparison == BooleanComparisonType.LessThanOrEqualTo)
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

        private struct FuncOnVar
        {
            public string FuncName;

            public string VarName;
        }
    }
}
