using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class OptionalParameterIsNullPredicateRule
    {
        private sealed class BadPredicateDetector : TSqlFragmentVisitor
        {
            public BadPredicateDetector(Action<TSqlFragment, string> callback)
            {
                Callback = callback;
            }

            private Action<TSqlFragment, string> Callback { get; }

            public override void Visit(BooleanComparisonExpression node)
            {
                if (node.ComparisonType == BooleanComparisonType.GreaterThan
                || node.ComparisonType == BooleanComparisonType.LessThan
                || node.ComparisonType == BooleanComparisonType.NotEqualToExclamation
                || node.ComparisonType == BooleanComparisonType.NotEqualToBrackets)
                {
                    // inequality does not seem to be equivalently replaceable with OR
                    return;
                }

                var left = ExpandExpression(node.FirstExpression);
                var right = ExpandExpression(node.SecondExpression);

                if (right is ColumnReferenceExpression)
                {
                    (left, right) = (right, left);
                }

                if (!(left is ColumnReferenceExpression filteredColumnReference))
                {
                    // neither of comparison sides is a column reference - not our case
                    return;
                }

                ScalarExpression optionalArgument = null;
                ScalarExpression argumentAlternative = null;

                if (right is FunctionCall isnull
                && isnull.Parameters.Count == 2
                && isnull.FunctionName.Value.Equals("ISNULL", StringComparison.OrdinalIgnoreCase))
                {
                    // tbl.col = ISNULL(@arg, tbl.col)
                    optionalArgument = isnull.Parameters[0];
                    argumentAlternative = isnull.Parameters[1];
                }
                else if (right is CoalesceExpression coalesce
                && coalesce.Expressions.Count >= 2)
                {
                    // tbl.col = COALESCE(@arg, tbl.col)
                    optionalArgument = coalesce.Expressions[0];
                    argumentAlternative = coalesce.Expressions[1];
                }
                else
                {
                    // not our case
                    return;
                }

                DetectConvertibleFunctionCall(filteredColumnReference, ExpandExpression(optionalArgument), ExpandExpression(argumentAlternative));
            }

            private static ScalarExpression ExpandExpression(ScalarExpression expr)
            {
                while (expr is ParenthesisExpression pe)
                {
                    expr = pe.Expression;
                }

                return expr;
            }

            private void DetectConvertibleFunctionCall(ColumnReferenceExpression col, ScalarExpression arg, ScalarExpression alt)
            {
                if (col?.MultiPartIdentifier is null)
                {
                    // e.g. $action system col
                    return;
                }

                if (!(alt is ColumnReferenceExpression altCol) || altCol.MultiPartIdentifier is null)
                {
                    return;
                }

                if (!(arg is VariableReference varRef))
                {
                    return;
                }

                string filteredColumnReference = col.GetFullName();
                if (!filteredColumnReference.Equals(altCol.GetFullName(), StringComparison.OrdinalIgnoreCase))
                {
                    // some other column referenced on the right side of comparison
                    return;
                }

                Callback(varRef, $"({filteredColumnReference} = {varRef.Name} OR {varRef.Name} IS NULL)");
            }
        }
    }
}
