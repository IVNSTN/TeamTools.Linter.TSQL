using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0959", "DUP_CONDITIONAL_FLOW")]
    internal sealed class DuplicateConditionRule : AbstractRule
    {
        private static readonly string MsgTemplate = "see another occurance at line {0}";

        public DuplicateConditionRule() : base()
        {
        }

        // Finding similar WHEN predicates
        public override void Visit(SimpleCaseExpression node)
        {
            if (node.WhenClauses.Count < 2)
            {
                return;
            }

            var foundConditions = new List<ScalarExpression>();
            foreach (var condition in node.WhenClauses)
            {
                AddOrRaiseViolation(
                    foundConditions,
                    ExtractExpression(condition.WhenExpression));
            }
        }

        // Finding similar input expressions
        public override void Visit(CoalesceExpression node)
        {
            var foundConditions = new List<ScalarExpression>();
            foreach (var condition in node.Expressions)
            {
                AddOrRaiseViolation(
                    foundConditions,
                    ExtractExpression(condition));
            }
        }

        // Finding similar WHEN predicates
        public override void Visit(SearchedCaseExpression node)
        {
            if (node.WhenClauses.Count < 2)
            {
                return;
            }

            var foundConditions = new List<BooleanExpressionParts>();
            foreach (var condition in node.WhenClauses)
            {
                AddOrRaiseViolation(
                    foundConditions,
                    NormalizeBooleanExpression(ExtractExpression(condition.WhenExpression)));
            }
        }

        // Finding similar conditions in alternative flows of IF-ELSE-IF
        public override void Visit(IfStatement node)
        {
            if (node.ElseStatement is null)
            {
                return;
            }

            var foundConditions = new List<BooleanExpressionParts>();
            foreach (var condition in GetAllIfFlows(node))
            {
                AddOrRaiseViolation(
                    foundConditions,
                    NormalizeBooleanExpression(ExtractExpression(condition)));
            }
        }

        // Finding NULLIF with equal first and second arguments
        public override void Visit(NullIfExpression node)
        {
            // similar left and right parts
            var foundConditions = new List<ScalarExpression>();

            AddOrRaiseViolation(
                foundConditions,
                ExtractExpression(node.FirstExpression));

            AddOrRaiseViolation(
                foundConditions,
                ExtractExpression(node.SecondExpression));
        }

        // Finding ISNULL with equal first and second arguments
        public override void Visit(FunctionCall node)
        {
            if (!node.FunctionName.Value.Equals("ISNULL", StringComparison.OrdinalIgnoreCase)
            || node.Parameters.Count != 2)
            {
                return;
            }

            // similar left and right parts
            var foundConditions = new List<ScalarExpression>();

            AddOrRaiseViolation(
                foundConditions,
                ExtractExpression(node.Parameters[0]));

            AddOrRaiseViolation(
                foundConditions,
                ExtractExpression(node.Parameters[1]));
        }

        // Finding similar parts of AND/OR binary expressions
        public override void Visit(BooleanBinaryExpression node)
        {
            var foundConditions = new List<BooleanExpressionParts>();
            var firstExpression = ExtractExpression(node.FirstExpression);
            var secondExpression = ExtractExpression(node.SecondExpression);

            AddOrRaiseViolation(
                foundConditions,
                NormalizeBooleanExpression(firstExpression));

            AddOrRaiseViolation(
                foundConditions,
                NormalizeBooleanExpression(secondExpression));

            // Going deeper to find similar expression part within complex expression
            // If we started with AND then type of nested expression does not matter
            // if we started with OR on top level then we go deeper only if nested expression is also OR
            // otherwise non-equal boolean expressions well be treated as equal, e.g.
            // WHERE tbl.start_date > GETDATE() AND (@a < @b) OR (@a > @b) OR ((@c = @d) AND (@a < @b))
            // here second instance of (@a < @b) must not be treated as duplicate.
            if (secondExpression is BooleanBinaryExpression binSecond
            && !(firstExpression is BooleanBinaryExpression)
            && (node.BinaryExpressionType == binSecond.BinaryExpressionType || node.BinaryExpressionType == BooleanBinaryExpressionType.And))
            {
                foreach (var nested in ExtractNestedExpressions(binSecond))
                {
                    AddOrRaiseViolation(
                        foundConditions,
                        NormalizeBooleanExpression(nested));
                }
            }

            if (firstExpression is BooleanBinaryExpression binFirst
            && !(secondExpression is BooleanBinaryExpression)
            && (node.BinaryExpressionType == binFirst.BinaryExpressionType || node.BinaryExpressionType == BooleanBinaryExpressionType.And))
            {
                foreach (var nested in ExtractNestedExpressions(binFirst))
                {
                    AddOrRaiseViolation(
                        foundConditions,
                        NormalizeBooleanExpression(nested));
                }
            }
        }

        // Extracting the expression from parenthesis or fake scalar select
        private static ScalarExpression ExtractExpression(ScalarExpression expr)
        {
            if (expr is ParenthesisExpression pe)
            {
                return ExtractExpression(pe.Expression);
            }

            if (expr is ScalarSubquery q)
            {
                var spec = q.QueryExpression.GetQuerySpecification();
                if (spec != null
                && spec.FromClause is null
                && spec.WhereClause is null
                && spec.SelectElements.Count == 1
                && spec.SelectElements[0] is SelectScalarExpression sel)
                {
                    // if subquery is selecting single column with no WHERE and FROM
                    // then we can grab selected scalar expression itself
                    return ExtractExpression(sel.Expression);
                }
            }

            return expr;
        }

        private static BooleanExpression ExtractExpression(BooleanExpression expr)
        {
            if (expr is BooleanParenthesisExpression pe)
            {
                return ExtractExpression(pe.Expression);
            }

            return expr;
        }

        // Extracting nested boolean expression if any
        private static IEnumerable<BooleanExpression> ExtractNestedExpressions(BooleanExpression node)
        {
            if (node is BooleanParenthesisExpression pe)
            {
                foreach (var e in ExtractNestedExpressions(pe.Expression))
                {
                    yield return e;
                }

                yield break;
            }

            if (node is BooleanBinaryExpression bin)
            {
                // once again, if the outer expression was connected with nested expression using AND
                // then any predicate found in the deep can be treated as duplicate
                // if we started with OR then we go deeper only if next expression is build with OR as well
                var firstExpr = ExtractExpression(bin.FirstExpression);
                if (!(firstExpr is BooleanBinaryExpression firstBin)
                || firstBin.BinaryExpressionType == bin.BinaryExpressionType
                || bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
                {
                    foreach (var e in ExtractNestedExpressions(firstExpr))
                    {
                        yield return e;
                    }
                }

                var secondExpr = ExtractExpression(bin.SecondExpression);
                if (!(secondExpr is BooleanBinaryExpression secondBin)
                || secondBin.BinaryExpressionType == bin.BinaryExpressionType
                || bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
                {
                    foreach (var e in ExtractNestedExpressions(secondExpr))
                    {
                        yield return e;
                    }
                }

                yield break;
            }

            yield return node;
        }

        // Normalizing boolean predicates to be able to detect equal expression even
        // if they are written in reversed or negated way
        // e.g. (@a < @b) is the same as (@b > @a) and NOT (@a >= @b)
        private static BooleanExpressionParts NormalizeBooleanExpression(BooleanExpression expr)
        {
            var result = new BooleanExpressionParts
            {
                OriginalExpression = expr,
            };

            // TODO : BooleanBinaryExpression? at least sort
            if (expr is BooleanComparisonExpression cmp)
            {
                // TODO : handle NotGreaterThan as negation
                result.ComparisonType = GetNormalizedComparisonType(cmp.ComparisonType);
                result.FirstExpression = ExtractExpression(cmp.FirstExpression);
                result.SecondExpression = ExtractExpression(cmp.SecondExpression);

                if (result.ComparisonType != cmp.ComparisonType)
                {
                    // switching sides of expression if needed for normalization
                    (result.FirstExpression, result.SecondExpression) = (result.SecondExpression, result.FirstExpression);
                }
                else if (result.ComparisonType == BooleanComparisonType.Equals)
                {
                    // normalizing by sorting alphabetically if comparison has no direction (< or >)
                    string firstExpression = result.FirstExpression.GetFragmentCleanedText();
                    string secondExpression = result.SecondExpression.GetFragmentCleanedText();
                    if (string.Compare(firstExpression, secondExpression) > 0)
                    {
                        // switching sides of expression if needed for normalization
                        (result.FirstExpression, result.SecondExpression) = (result.SecondExpression, result.FirstExpression);
                    }
                }
                else if (result.ComparisonType == BooleanComparisonType.NotEqualToExclamation)
                {
                    // replacing with equal comparison type for similarity
                    // !=  ->  <>
                    result.ComparisonType = BooleanComparisonType.NotEqualToBrackets;
                }
            }

            if (expr is BooleanNotExpression not)
            {
                // expanding NOT (@a >= @b) to @a < @b and normalizing to @b > @a
                result = NormalizeBooleanExpression(ExtractExpression(not.Expression));
                var comparisonType = GetNormalizedComparisonType(GetNegatedComparisonType(result.ComparisonType));
                if (comparisonType != result.ComparisonType)
                {
                    result.ComparisonType = comparisonType;
                    // switching sides of expression if needed for normalization
                    (result.FirstExpression, result.SecondExpression) = (result.SecondExpression, result.FirstExpression);
                }
            }

            return result;
        }

        // Normalizing comparison types: all left-sided (<, <=, !<) to right-sided (>, >=, !>)
        private static BooleanComparisonType GetNormalizedComparisonType(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.LessThan:
                    return BooleanComparisonType.GreaterThan;

                case BooleanComparisonType.LessThanOrEqualTo:
                    return BooleanComparisonType.GreaterThanOrEqualTo;

                case BooleanComparisonType.NotLessThan:
                    return BooleanComparisonType.NotGreaterThan;

                default:
                    return cmp;
            }
        }

        private static BooleanComparisonType GetNegatedComparisonType(BooleanComparisonType cmp)
        {
            switch (cmp)
            {
                case BooleanComparisonType.LessThan:
                    return BooleanComparisonType.GreaterThanOrEqualTo;

                case BooleanComparisonType.GreaterThan:
                    return BooleanComparisonType.LessThanOrEqualTo;

                case BooleanComparisonType.LessThanOrEqualTo:
                    return BooleanComparisonType.GreaterThan;

                case BooleanComparisonType.GreaterThanOrEqualTo:
                    return BooleanComparisonType.LessThan;

                case BooleanComparisonType.NotGreaterThan:
                    return BooleanComparisonType.LessThanOrEqualTo;

                case BooleanComparisonType.NotLessThan:
                    return BooleanComparisonType.GreaterThanOrEqualTo;

                case BooleanComparisonType.Equals:
                    return BooleanComparisonType.NotEqualToBrackets;

                case BooleanComparisonType.NotEqualToBrackets:
                    return BooleanComparisonType.Equals;

                case BooleanComparisonType.NotEqualToExclamation:
                    return BooleanComparisonType.Equals;

                default:
                    return cmp;
            }
        }

        // Going deeper to extract all the IF-ELSE-IF-ELSE-IF
        private static IEnumerable<BooleanExpression> GetAllIfFlows(IfStatement node)
        {
            yield return node.Predicate;

            IfStatement elseIf = GetElseIf(node);

            while (elseIf != null)
            {
                yield return elseIf.Predicate;

                elseIf = GetElseIf(elseIf);
            }
        }

        private static IfStatement GetElseIf(IfStatement node)
        {
            if (node is null)
            {
                return default;
            }

            if (node.ElseStatement is null)
            {
                return default;
            }

            if (node.ElseStatement is IfStatement ifstmt)
            {
                return ifstmt;
            }

            return default;
        }

        private static bool AreEqualExpressions(TSqlFragment exprA, TSqlFragment exprB)
        {
            if (exprA is null && exprB is null)
            {
                // both are nulls
                return true;
            }

            if (exprA is null || exprB is null)
            {
                // only one is null
                return false;
            }

            if (exprA is Literal literA && exprB is Literal literB)
            {
                // this is faster than building script fragment text
                return literA.Value.Equals(literB.Value, StringComparison.OrdinalIgnoreCase);
            }

            if (exprA is Literal || exprB is Literal)
            {
                // one is a literal another is not
                return false;
            }

            if (exprA is VariableReference varA && exprB is VariableReference varB)
            {
                // this is faster than building script fragment text
                return varA.Name.Equals(varB.Name, StringComparison.OrdinalIgnoreCase);
            }

            if (exprA is VariableReference || exprB is VariableReference)
            {
                // one is a variable reference another is not
                return false;
            }

            // comparing expressions lexicographically since there is no smarter option for now
            // TODO : something better and faster needed
            return string.Equals(exprA.GetFragmentCleanedText(), exprB.GetFragmentCleanedText(), StringComparison.OrdinalIgnoreCase);
        }

        private static bool AreEqualExpressions(BooleanExpressionParts exprA, BooleanExpressionParts exprB)
        {
            return exprA.ComparisonType == exprB.ComparisonType
                && AreEqualExpressions(exprA.FirstExpression, exprB.FirstExpression)
                && AreEqualExpressions(exprA.SecondExpression, exprB.SecondExpression)
                && ((exprA.FirstExpression != null && exprA.SecondExpression != null)
                    || AreEqualExpressions(exprA.OriginalExpression, exprB.OriginalExpression));
        }

        private void AddOrRaiseViolation(IList<ScalarExpression> registeredItems, ScalarExpression newItem)
        {
            if (newItem is null)
            {
                return;
            }

            TSqlFragment firstInstance = default;

            foreach (var oldItem in registeredItems)
            {
                if (AreEqualExpressions(oldItem, newItem))
                {
                    // if we already registered similar expression
                    // then newItem is the duplicate we are looking for
                    firstInstance = oldItem;
                    break;
                }
            }

            if (firstInstance is null)
            {
                // otherwise this is a brand new expression
                // registering it to find duplicates in the rest of the code
                registeredItems.Add(newItem);
                return;
            }

            HandleNodeError(newItem, string.Format(MsgTemplate, firstInstance.StartLine));
        }

        // TODO : looks like a generic method
        private void AddOrRaiseViolation(IList<BooleanExpressionParts> registeredItems, BooleanExpressionParts newItem)
        {
            if (newItem is null)
            {
                return;
            }

            BooleanExpressionParts firstInstance = default;

            foreach (var oldItem in registeredItems)
            {
                if (AreEqualExpressions(oldItem, newItem))
                {
                    firstInstance = oldItem;
                    break;
                }
            }

            if (firstInstance is null)
            {
                registeredItems.Add(newItem);
                return;
            }

            HandleNodeError(
                newItem.FirstExpression as TSqlFragment ?? newItem.OriginalExpression,
                string.Format(MsgTemplate, (firstInstance.FirstExpression as TSqlFragment ?? firstInstance.OriginalExpression).StartLine));
        }

        private class BooleanExpressionParts
        {
            public ScalarExpression FirstExpression { get; set; }

            public ScalarExpression SecondExpression { get; set; }

            public BooleanExpression OriginalExpression { get; set; }

            public BooleanComparisonType ComparisonType { get; set; } = BooleanComparisonType.Equals;
        }
    }
}
