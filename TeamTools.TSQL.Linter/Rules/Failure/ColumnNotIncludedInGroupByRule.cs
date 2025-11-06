using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : no GROUP BY but Aggregate function used -> this is the same case actually just no columns are grouped
    [RuleIdentity("FA0949", "COLUMN_NOT_IN_GROUP_BY")]
    internal sealed class ColumnNotIncludedInGroupByRule : AbstractRule, ISqlServerMetadataConsumer
    {
        private readonly ICollection<string> nonColumnIdentifiers = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public ColumnNotIncludedInGroupByRule() : base()
        {
        }

        // TODO : load AGGREGATE and WINDOW(?) functions
        public void LoadMetadata(SqlServerMetadata data)
        {
            nonColumnIdentifiers.Clear();
            if (data.Enums.ContainsKey(TSqlDomainAttributes.DateTimePartEnum))
            {
                foreach (var datepart in data.Enums[TSqlDomainAttributes.DateTimePartEnum])
                {
                    nonColumnIdentifiers.Add(datepart.Name);
                }
            }
        }

        public override void Visit(QuerySpecification node)
        {
            if (node.GroupByClause is null)
            {
                return;
            }

            var groupedExpressions = new List<string>();
            var selectedExpressions = new List<string>();

            foreach (var grp in node.GroupByClause.GroupingSpecifications)
            {
                if (!(grp is ExpressionGroupingSpecification grpExpr))
                {
                    // unsupported case
                    return;
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

                    string colName = colRef.MultiPartIdentifier.Identifiers.Last().Value;

                    if (groupedExpressions.Contains(colName))
                    {
                        continue;
                    }

                    // TODO : validate column belonging
                    // also registering column name without alias
                    // because the rule currently is not able to check all the aliases
                    groupedExpressions.Add(colName);
                }
            }

            var colVisitor = new ColumnReferenceVisitor(nonColumnIdentifiers);

            foreach (var col in node.SelectElements)
            {
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
                if (groupedExpressions.Any(exprText => exprText.Equals(selText, StringComparison.OrdinalIgnoreCase)))
                {
                    // same expression exists in GROUP BY clause
                    continue;
                }

                // TODO : if there is only one column in the whole expression
                // which is the issue then report this column name only
                HandleNodeError(col, selText);
            }
        }

        private static string GetExpressionDefinitionText(ScalarExpression expr)
        {
            if (expr is null)
            {
                return default;
            }

            if (expr is ParenthesisExpression pe)
            {
                return GetExpressionDefinitionText(pe.Expression);
            }

            return expr.GetFragmentCleanedText();
        }

        private static bool ContainsColumnReference(ScalarExpression expr, ColumnReferenceVisitor visitor)
        {
            visitor.Reset();
            expr.Accept(visitor);
            return visitor.Detected;
        }

        private bool IsInvalidInSelect(ScalarExpression node, ICollection<string> groupedExpressions)
        {
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
                return clsc.Expressions.Select(e => IsInvalidInSelect(e, groupedExpressions)).Max();
            }

            if (node is SearchedCaseExpression searchedCase)
            {
                return searchedCase.WhenClauses.Select(e => IsInvalidInSelect(e.WhenExpression, groupedExpressions)).Max()
                    || searchedCase.WhenClauses.Select(e => IsInvalidInSelect(e.ThenExpression, groupedExpressions)).Max()
                    || IsInvalidInSelect(searchedCase.ElseExpression, groupedExpressions);
            }

            if (node is SimpleCaseExpression simpleCase)
            {
                return simpleCase.WhenClauses.Select(e => IsInvalidInSelect(e.WhenExpression, groupedExpressions)).Max()
                    || simpleCase.WhenClauses.Select(e => IsInvalidInSelect(e.ThenExpression, groupedExpressions)).Max()
                    || IsInvalidInSelect(simpleCase.InputExpression, groupedExpressions)
                    || IsInvalidInSelect(simpleCase.ElseExpression, groupedExpressions);
            }

            // TODO : allow only AGGREGATE functions and UDF (they could be aggregate fn too)
            if (node is FunctionCall funcCall)
            {
                if (funcCall.FunctionName.Value.Equals("ISNULL", StringComparison.OrdinalIgnoreCase))
                {
                    return funcCall.Parameters.Select(p => IsInvalidInSelect(p, groupedExpressions)).Max();
                }

                return false;
            }

            if (node is ColumnReferenceExpression colRef)
            {
                if (colRef.ColumnType == ColumnType.Wildcard)
                {
                    return true;
                }

                if (nonColumnIdentifiers.Contains(colRef.MultiPartIdentifier.Identifiers.Last().Value))
                {
                    return false;
                }

                return !groupedExpressions.Contains(colRef.GetFragmentCleanedText());
            }

            return false;
        }

        private bool IsInvalidInSelect(BooleanExpression node, ICollection<string> groupedExpressions)
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

        private class ColumnReferenceVisitor : TSqlViolationDetector
        {
            private readonly ICollection<string> nonColumnIdentifiers;

            public ColumnReferenceVisitor(ICollection<string> nonColumnIdentifiers)
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
