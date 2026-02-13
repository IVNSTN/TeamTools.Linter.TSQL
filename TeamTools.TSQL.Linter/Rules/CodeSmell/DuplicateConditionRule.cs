using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0959", "DUP_CONDITIONAL_FLOW")]
    internal sealed class DuplicateConditionRule : AbstractRule
    {
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
            int n = node.WhenClauses.Count;
            for (int i = 0; i < n; i++)
            {
                AddOrRaiseViolation(
                    foundConditions,
                    ExtractExpression(node.WhenClauses[i].WhenExpression));
            }
        }

        // Finding similar input expressions
        public override void Visit(CoalesceExpression node)
        {
            var foundConditions = new List<ScalarExpression>();
            int n = node.Expressions.Count;
            for (int i = 0; i < n; i++)
            {
                AddOrRaiseViolation(
                    foundConditions,
                    ExtractExpression(node.Expressions[i]));
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
            int n = node.WhenClauses.Count;
            for (int i = 0; i < n; i++)
            {
                AddOrRaiseViolation(
                    foundConditions,
                    NormalizeBooleanExpression(ExtractExpression(node.WhenClauses[i].WhenExpression)));
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

        // Extracting nested boolean expression if any
        private static IEnumerable<BooleanExpression> ExtractNestedExpressions(BooleanExpression node)
        {
            node = ExtractExpression(node);

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

        private static BooleanExpressionParts NormalizeBooleanExpression(BooleanExpression expr)
            => BooleanExpressionNormalizer.Normalize(expr);

        // Extracting the expression from parenthesis or fake scalar select
        private static ScalarExpression ExtractExpression(ScalarExpression expr)
            => BooleanExpressionPartsExtractor.ExtractExpression(expr);

        private static BooleanExpression ExtractExpression(BooleanExpression expr)
            => BooleanExpressionPartsExtractor.ExtractExpression(expr);

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
            if (node?.ElseStatement is IfStatement ifstmt)
            {
                return ifstmt;
            }

            return default;
        }

        private void AddOrRaiseViolation(IList<ScalarExpression> registeredItems, ScalarExpression newItem)
        {
            if (newItem is null)
            {
                return;
            }

            TSqlFragment firstInstance = default;

            int n = registeredItems.Count;
            for (int i = 0; i < n; i++)
            {
                var oldItem = registeredItems[i];
                if (BooleanExpressionComparer.AreEqualExpressions(oldItem, newItem))
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

            HandleNodeError(newItem, string.Format(Strings.ViolationDetails_DuplicateConditionRule_SeeDupAtLine, firstInstance.StartLine));
        }

        // TODO : looks like a generic method
        private void AddOrRaiseViolation(IList<BooleanExpressionParts> registeredItems, BooleanExpressionParts newItem)
        {
            if (newItem is null)
            {
                return;
            }

            BooleanExpressionParts firstInstance = default;

            int n = registeredItems.Count;
            for (int i = 0; i < n; i++)
            {
                var oldItem = registeredItems[i];
                if (BooleanExpressionComparer.AreEqualExpressions(oldItem, newItem))
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
                string.Format(Strings.ViolationDetails_DuplicateConditionRule_SeeDupAtLine, (firstInstance.FirstExpression as TSqlFragment ?? firstInstance.OriginalExpression).StartLine));
        }
    }
}
