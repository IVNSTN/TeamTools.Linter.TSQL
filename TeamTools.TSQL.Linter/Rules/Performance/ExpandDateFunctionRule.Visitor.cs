using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ExpandDateFunctionRule
    {
        private sealed class BadPredicateDetector : TSqlFragmentVisitor
        {
            public BadPredicateDetector(Action<TSqlFragment, string> callback)
            {
                Callback = callback;
            }

            private Action<TSqlFragment, string> Callback { get; }

            // TODO : BooleanTernaryExpression (BETWEEN), InPredicate
            public override void Visit(BooleanComparisonExpression node)
            {
                var left = ExpandExpression(node.FirstExpression);
                var right = ExpandExpression(node.SecondExpression);

                if (left is VariableReference || left is Literal)
                {
                    (left, right) = (right, left);
                }
                else if (!(right is VariableReference || right is Literal))
                {
                    // expression cannot be converted into sargable datetime range predicate
                    return;
                }

                if (!(left is FunctionCall func))
                {
                    return;
                }

                DetectConvertibleFunctionCall(func);
            }

            private static ScalarExpression ExpandExpression(ScalarExpression expr)
            {
                while (expr is ParenthesisExpression pe)
                {
                    expr = pe.Expression;
                }

                return expr;
            }

            private static bool CanBePrecomputed(ScalarExpression expr)
            {
                expr = expr.ExtractScalarExpression();
                return expr is Literal || expr is VariableReference;
            }

            private static bool IsConvertibleDatePartCall(FunctionCall func)
            {
                if (func.Parameters.Count != 2)
                {
                    // broken syntax
                    return false;
                }

                if (CanBePrecomputed(func.Parameters[1]))
                {
                    // expression can be precomputed
                    return false;
                }

                // only YEAR can be easily converted to range filter
                return ExpandExpression(func.Parameters[0]) is ColumnReferenceExpression col
                    && col.MultiPartIdentifier.GetLastIdentifier().Value.Equals("YEAR", StringComparison.OrdinalIgnoreCase);
            }

            private void DetectConvertibleFunctionCall(FunctionCall func)
            {
                string funcName = func.FunctionName.Value;

                if (funcName.Equals("YEAR", StringComparison.OrdinalIgnoreCase))
                {
                    if (func.Parameters.Count != 1 || CanBePrecomputed(func.Parameters[0]))
                    {
                        return;
                    }

                    Callback(func, "YEAR");
                }
                else if (funcName.Equals("DATETRUNC", StringComparison.OrdinalIgnoreCase))
                {
                    if (func.Parameters.Count != 2 || CanBePrecomputed(func.Parameters[1]))
                    {
                        return;
                    }

                    Callback(func, "DATETRUNC");
                }
                else if (funcName.Equals("DATEPART", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsConvertibleDatePartCall(func))
                    {
                        Callback(func, "DATEPART");
                    }
                }
                else if (funcName.Equals("DATENAME", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsConvertibleDatePartCall(func))
                    {
                        Callback(func, "DATENAME");
                    }
                }
                else if (funcName.Equals("DATEADD", StringComparison.OrdinalIgnoreCase))
                {
                    if (func.Parameters.Count != 3
                    || (CanBePrecomputed(func.Parameters[1]) && CanBePrecomputed(func.Parameters[2])))
                    {
                        return;
                    }

                    Callback(func, "DATEADD");
                }
            }
        }
    }
}
