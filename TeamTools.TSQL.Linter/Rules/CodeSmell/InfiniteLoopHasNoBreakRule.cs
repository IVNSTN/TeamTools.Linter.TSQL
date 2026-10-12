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
            while (expr is BooleanParenthesisExpression pe)
            {
                expr = pe.Expression;
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
                return IsAlwaysTrueComparison(cmp);
            }

            return false;
        }

        private static bool IsAlwaysTrueComparison(BooleanComparisonExpression cmp)
        {
            var valueA = cmp.FirstExpression.ExtractScalarExpression();
            var valueB = cmp.SecondExpression.ExtractScalarExpression();

            if (valueA is null || valueB is null)
            {
                return false;
            }

            var expressionsAreEqual = AreEqual(valueA, valueB);

            if (cmp.ComparisonType == BooleanComparisonType.Equals)
            {
                // same literals or expressions
                return expressionsAreEqual;
            }

            if (!(valueA is Literal literA && valueB is Literal literB))
            {
                // unable to say anything specific for non-constant values
                return false;
            }

            switch (cmp.ComparisonType)
            {
                case BooleanComparisonType.NotEqualToBrackets:
                case BooleanComparisonType.NotEqualToExclamation:
                    return !expressionsAreEqual;

                case BooleanComparisonType.GreaterThan:
                    // TODO : these comparisons ignore actual literal type and implicit conversion rules
                    return string.Compare(literA.Value, literB.Value) > 0;

                case BooleanComparisonType.LessThan:
                    return string.Compare(literA.Value, literB.Value) < 0;

                case BooleanComparisonType.GreaterThanOrEqualTo:
                case BooleanComparisonType.NotLessThan:
                    return expressionsAreEqual || string.Compare(literA.Value, literB.Value) > 0;

                case BooleanComparisonType.LessThanOrEqualTo:
                case BooleanComparisonType.NotGreaterThan:
                    return expressionsAreEqual || string.Compare(literA.Value, literB.Value) < 0;

                default:
                    return false;
            }
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

        // TODO : check if breaking statement is hidden under always false condition
        private sealed class BreakVisitor : TSqlViolationDetector
        {
            public override void Visit(BreakStatement node) => MarkDetected(node);

            public override void Visit(ReturnStatement node) => MarkDetected(node);

            public override void Visit(ThrowStatement node) => MarkDetected(node);
        }
    }
}
