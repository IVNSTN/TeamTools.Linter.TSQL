using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : no GROUP BY but Aggregate function used -> this is the same case actually just no columns are grouped
    [RuleIdentity("FA0949", "COLUMN_NOT_IN_GROUP_BY")]
    internal sealed class ColumnNotIncludedInGroupByRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private HashSet<string> nonColumnIdentifiers;

        public ColumnNotIncludedInGroupByRule() : base()
        {
        }

        // TODO : load AGGREGATE and WINDOW(?) functions
        public void LoadMetadata(SqlServerMetadata data)
        {
            if (data.Enums.TryGetValue(TSqlDomainAttributes.DateTimePartEnum, out var dateParts))
            {
                nonColumnIdentifiers = new HashSet<string>(dateParts.Select(dtp => dtp.Name), StringComparer.OrdinalIgnoreCase);
            }
        }

        public override void Visit(QuerySpecification node)
        {
            if (node.GroupByClause is null)
            {
                return;
            }

            var groupedExpressions = ExtractGroupedExpressions(node.GroupByClause.GroupingSpecifications);
            if (groupedExpressions.Count > 0)
            {
                ValidateSelectedExpressions(node.SelectElements, groupedExpressions);
            }
        }

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

        private HashSet<string> ExtractGroupedExpressions(IList<GroupingSpecification> groupby)
        {
            var groupedExpressions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int n = groupby.Count;
            for (int i = 0; i < n; i++)
            {
                var grp = groupby[i];
                if (!(grp is ExpressionGroupingSpecification grpExpr))
                {
                    // unsupported case
                    break;
                }

                if (grpExpr.Expression is ValueExpression)
                {
                    // not interested in vars ans literals
                    continue;
                }

                groupedExpressions.Add(grpExpr.Expression.GetFragmentCleanedText());

                if (grpExpr.Expression is ColumnReferenceExpression colRef)
                {
                    if (colRef.ColumnType == ColumnType.Wildcard)
                    {
                        continue;
                    }

                    string colName = colRef.MultiPartIdentifier.GetLastIdentifier().Value;

                    // TODO : validate column belonging
                    // also registering column name without alias
                    // because the rule currently is not able to check all the aliases
                    groupedExpressions.Add(colName);
                }
            }

            return groupedExpressions;
        }

        private void ValidateSelectedExpressions(IList<SelectElement> selected, HashSet<string> groupedExpressions)
        {
            var colVisitor = new ColumnReferenceVisitor(nonColumnIdentifiers);

            int n = selected.Count;
            for (int i = 0; i < n; i++)
            {
                var col = selected[i];
                if (!(col is SelectScalarExpression selExpr))
                {
                    // not interested in vars ans literals
                    continue;
                }

                if (!ContainsColumnReference(selExpr.Expression, colVisitor))
                {
                    continue;
                }

                if (!IsInvalidInSelect(selExpr.Expression, groupedExpressions))
                {
                    continue;
                }

                string selText = GetExpressionDefinitionText(selExpr.Expression);
                if (groupedExpressions.Contains(selText))
                {
                    // same expression exists in GROUP BY clause
                    continue;
                }

                // TODO : if there is only one column in the whole expression
                // which is the issue then report this column name only
                HandleNodeError(col, selText);
            }
        }

        private sealed class ColumnReferenceVisitor : TSqlViolationDetector
        {
            private readonly HashSet<string> nonColumnIdentifiers;

            public ColumnReferenceVisitor(HashSet<string> nonColumnIdentifiers)
            {
                this.nonColumnIdentifiers = nonColumnIdentifiers;
            }

            public void Reset() => Detected = false;

            public override void Visit(ColumnReferenceExpression node)
            {
                if (node.ColumnType == ColumnType.Wildcard)
                {
                    return;
                }

                if (node.MultiPartIdentifier.Identifiers.Count == 1
                && nonColumnIdentifiers.Contains(node.MultiPartIdentifier.Identifiers[0].Value))
                {
                    return;
                }

                MarkDetected(node);
            }
        }
    }
}
