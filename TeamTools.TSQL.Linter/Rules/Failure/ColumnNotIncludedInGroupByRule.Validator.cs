using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : no GROUP BY but Aggregate function used -> this is the same case actually just no columns are grouped
    internal partial class ColumnNotIncludedInGroupByRule
    {
        private static string GetExpressionDefinitionText(ScalarExpression expr)
        {
            if (expr is null)
            {
                return default;
            }

            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            return expr.GetFragmentCleanedText();
        }

        private static bool ContainsColumnReference(ScalarExpression expr, ColumnReferenceVisitor visitor)
        {
            visitor.Reset();
            expr.Accept(visitor);
            return visitor.Detected;
        }

        private bool ContainsInvalidExpression(IList<ScalarExpression> expressions, HashSet<string> groupedExpressions)
        {
            int n = expressions.Count;
            for (int i = 0; i < n; i++)
            {
                var expr = expressions[i];
                if (IsInvalidInSelect(expr, groupedExpressions))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsInvalidExpression(IList<SearchedWhenClause> expressions, HashSet<string> groupedExpressions)
        {
            int n = expressions.Count;
            for (int i = 0; i < n; i++)
            {
                var expr = expressions[i];
                if (IsInvalidInSelect(expr.WhenExpression, groupedExpressions)
                || IsInvalidInSelect(expr.ThenExpression, groupedExpressions))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsInvalidExpression(IList<SimpleWhenClause> expressions, HashSet<string> groupedExpressions)
        {
            int n = expressions.Count;
            for (int i = 0; i < n; i++)
            {
                var expr = expressions[i];
                if (IsInvalidInSelect(expr.WhenExpression, groupedExpressions)
                || IsInvalidInSelect(expr.ThenExpression, groupedExpressions))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsInvalidInSelect(ScalarExpression node, HashSet<string> groupedExpressions)
        {
            Debug.Assert(nonColumnIdentifiers != null && nonColumnIdentifiers.Count > 0, "nonColumnIdentifiers not loaded");

            if (node is null)
            {
                return false;
            }

            // TODO : save flag if groupedExpressions contains expressions and compute full node text only if it does
            if (groupedExpressions.Contains(GetExpressionDefinitionText(node)))
            {
                return false;
            }

            if (node is ParenthesisExpression pe)
            {
                return IsInvalidInSelect(pe.Expression, groupedExpressions);
            }

            if (node is BinaryExpression bin)
            {
                return IsInvalidInSelect(bin.FirstExpression, groupedExpressions)
                    || IsInvalidInSelect(bin.SecondExpression, groupedExpressions);
            }

            if (node is UnaryExpression un)
            {
                return IsInvalidInSelect(un.Expression, groupedExpressions);
            }

            if (node is ValueExpression)
            {
                return false;
            }

            if (node is IIfCall iif)
            {
                return IsInvalidInSelect(iif.Predicate, groupedExpressions)
                    || IsInvalidInSelect(iif.ThenExpression, groupedExpressions)
                    || IsInvalidInSelect(iif.ElseExpression, groupedExpressions);
            }

            if (node is CastCall castCall)
            {
                return IsInvalidInSelect(castCall.Parameter, groupedExpressions);
            }

            if (node is ConvertCall convertCall)
            {
                return IsInvalidInSelect(convertCall.Parameter, groupedExpressions);
            }

            if (node is NullIfExpression nlif)
            {
                return IsInvalidInSelect(nlif.FirstExpression, groupedExpressions)
                    || IsInvalidInSelect(nlif.SecondExpression, groupedExpressions);
            }

            if (node is CoalesceExpression clsc)
            {
                return ContainsInvalidExpression(clsc.Expressions, groupedExpressions);
            }

            if (node is SearchedCaseExpression searchedCase)
            {
                return ContainsInvalidExpression(searchedCase.WhenClauses, groupedExpressions)
                    || IsInvalidInSelect(searchedCase.ElseExpression, groupedExpressions);
            }

            if (node is SimpleCaseExpression simpleCase)
            {
                return ContainsInvalidExpression(simpleCase.WhenClauses, groupedExpressions)
                    || IsInvalidInSelect(simpleCase.InputExpression, groupedExpressions)
                    || IsInvalidInSelect(simpleCase.ElseExpression, groupedExpressions);
            }

            // TODO : allow only AGGREGATE functions and UDF (they could be aggregate fn too)
            if (node is FunctionCall funcCall)
            {
                if (funcCall.FunctionName.Value.Equals("ISNULL", StringComparison.OrdinalIgnoreCase))
                {
                    return ContainsInvalidExpression(funcCall.Parameters, groupedExpressions);
                }

                return false;
            }

            if (node is ColumnReferenceExpression colRef)
            {
                if (colRef.ColumnType == ColumnType.Wildcard)
                {
                    return true;
                }

                if (nonColumnIdentifiers.Contains(colRef.MultiPartIdentifier.GetLastIdentifier().Value))
                {
                    return false;
                }

                return !groupedExpressions.Contains(colRef.GetFragmentCleanedText());
            }

            return false;
        }

        private bool IsInvalidInSelect(BooleanExpression node, HashSet<string> groupedExpressions)
        {
            if (node is BooleanParenthesisExpression pe)
            {
                return IsInvalidInSelect(pe.Expression, groupedExpressions);
            }

            if (node is BooleanBinaryExpression bin)
            {
                return IsInvalidInSelect(bin.FirstExpression, groupedExpressions)
                    || IsInvalidInSelect(bin.SecondExpression, groupedExpressions);
            }

            if (node is BooleanComparisonExpression cmp)
            {
                return IsInvalidInSelect(cmp.FirstExpression, groupedExpressions)
                    || IsInvalidInSelect(cmp.SecondExpression, groupedExpressions);
            }

            return false;
        }
    }
}
