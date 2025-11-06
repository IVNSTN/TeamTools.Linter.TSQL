using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0955", "LOOP_INFINITE_MISSING_BREAK")]
    internal sealed class InfiniteLoopHasNoBreakRule : AbstractRule
    {
        public InfiniteLoopHasNoBreakRule() : base()
        {
        }

        public override void Visit(WhileStatement node)
        {
            var breakVisitor = new BreakVisitor();
            node.Statement.Accept(breakVisitor);

            if (breakVisitor.Detected)
            {
                return;
            }

            // predicate analysis looks more complicated thus doing this
            // only when no exit from loop detected
            if (!IsAlwaysTrueCondition(node.Predicate))
            {
                return;
            }

            HandleNodeError(node.Predicate);
        }

        private static bool IsAlwaysTrueCondition(BooleanExpression expr)
        {
            if (expr is BooleanParenthesisExpression pe)
            {
                return IsAlwaysTrueCondition(pe.Expression);
            }

            if (expr is BooleanBinaryExpression bin)
            {
                if (bin.BinaryExpressionType == BooleanBinaryExpressionType.And)
                {
                    return IsAlwaysTrueCondition(bin.FirstExpression)
                        && IsAlwaysTrueCondition(bin.SecondExpression);
                }
                else
                {
                    return IsAlwaysTrueCondition(bin.FirstExpression)
                        || IsAlwaysTrueCondition(bin.SecondExpression);
                }
            }

            if (expr is BooleanComparisonExpression cmp)
            {
                var valueA = GetScalarExpression(cmp.FirstExpression);
                var valueB = GetScalarExpression(cmp.SecondExpression);

                if (valueA is null || valueB is null)
                {
                    return false;
                }

                if (cmp.ComparisonType == BooleanComparisonType.Equals)
                {
                    return AreEqual(valueA, valueB);
                }

                // TODO : support >, < and so on
                if (cmp.ComparisonType.In(BooleanComparisonType.NotEqualToBrackets, BooleanComparisonType.NotEqualToExclamation))
                {
                    return valueA is Literal
                        && valueB is Literal
                        && !AreEqual(valueA, valueB);
                }
            }

            return false;
        }

        private static bool AreEqual(ScalarExpression valueA, ScalarExpression valueB)
        {
            string valueInterpretationA = GetValueInterpretation(valueA);
            string valueInterpretationB = GetValueInterpretation(valueB);

            if (string.IsNullOrEmpty(valueInterpretationA) || string.IsNullOrEmpty(valueInterpretationB))
            {
                return false;
            }

            return string.Equals(valueInterpretationA, valueInterpretationB, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetValueInterpretation(ScalarExpression expr)
        {
            if (expr is null)
            {
                return default;
            }

            if (expr is Literal lit)
            {
                return lit.Value;
            }

            if (expr is VariableReference varRef)
            {
                return varRef.Name;
            }

            if (expr is GlobalVariableExpression glob)
            {
                return glob.Name;
            }

            return expr.GetFragmentCleanedText();
        }

        private static ScalarExpression GetScalarExpression(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is ScalarSubquery sel)
            {
                var spec = sel.QueryExpression.GetQuerySpecification();

                if (spec is null || spec.SelectElements.Count != 1
                || !(spec.SelectElements[0] is SelectScalarExpression selScalar))
                {
                    // something strange
                    return default;
                }

                return GetScalarExpression(selScalar.Expression);
            }

            return expr;
        }

        // TODO : check if breaking statement is hidden under always false condition
        private class BreakVisitor : TSqlViolationDetector
        {
            public override void Visit(BreakStatement node) => MarkDetected(node);

            public override void Visit(ReturnStatement node) => MarkDetected(node);

            public override void Visit(ThrowStatement node) => MarkDetected(node);
        }
    }
}
