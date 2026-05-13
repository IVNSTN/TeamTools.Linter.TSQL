using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Text;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ExtractWindowClauseRule
    {
        private sealed class OverClauseVisitor : VisitorWithCallback
        {
            private Dictionary<string, TSqlFragment> windows;

            public OverClauseVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(OverClause node)
            {
                if (node.WindowName != null)
                {
                    // already using external window definition referenced by name
                    return;
                }

                string windowDefinition = GetWindowDefinition(node);

                if (string.IsNullOrEmpty(windowDefinition))
                {
                    return;
                }

                if (windows is null)
                {
                    // Postponed creation because not every query has an OVER clause
                    windows = new Dictionary<string, TSqlFragment>(StringComparer.OrdinalIgnoreCase);
                }

                if (!windows.TryAdd(windowDefinition, node))
                {
                    Callback(node);
                }
            }

            // To prevent detection of windows already extrected to WINDOW clause
            public override void ExplicitVisit(WindowDefinition node)
            { }

            // To prevent diving into nested subqueries which have different scope
            public override void ExplicitVisit(QueryDerivedTable node)
            { }

            public override void ExplicitVisit(QueryParenthesisExpression node)
            { }

            public override void ExplicitVisit(ScalarSubquery node)
            { }

            private static string GetWindowDefinition(OverClause node)
            {
                var sb = new StringBuilder();

                if (node.Partitions != null && node.Partitions.Count > 0)
                {
                    AppendPartitionsInfo(sb, node.Partitions);
                }

                if (node.OrderByClause != null)
                {
                    AppendOrderByInfo(sb, node.OrderByClause.OrderByElements);
                }

                if (node.WindowFrameClause != null)
                {
                    AppendWindowFrameInfo(sb, node.WindowFrameClause);
                }

                return sb.ToString();
            }

            private static void AppendOrderByInfo(StringBuilder sb, IList<ExpressionWithSortOrder> orderBy)
            {
                if (sb.Length > 0)
                {
                    // ORDER BY can be defined without PARTITION BY clause before it
                    sb.Append(' ');
                }

                sb.Append("ORDER BY");

                for (int i = 0, n = orderBy.Count; i < n; i++)
                {
                    var orderedElement = orderBy[i];
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    sb.Append(' ');

                    sb.Append(GetExpressionText(orderedElement.Expression));

                    // ASC is the default and can be omitted
                    if (orderedElement.SortOrder == SortOrder.Descending)
                    {
                        sb.Append(" DESC");
                    }
                }
            }

            private static void AppendPartitionsInfo(StringBuilder sb, IList<ScalarExpression> partitions)
            {
                sb.Append("PARTITION BY");

                for (int i = 0, n = partitions.Count; i < n; i++)
                {
                    var partitionby = partitions[i];
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    sb.Append(' ');

                    sb.Append(GetExpressionText(partitionby));
                }
            }

            private static void AppendWindowFrameInfo(StringBuilder sb, WindowFrameClause windowFrame)
            {
                if (windowFrame.WindowFrameType == WindowFrameType.Range
                && windowFrame.Top.WindowDelimiterType == WindowDelimiterType.UnboundedPreceding
                && windowFrame.Bottom.WindowDelimiterType == WindowDelimiterType.CurrentRow)
                {
                    // The default frame declaration can be omitted:
                    // RANGE BETWEEN UNBOUND PRECEDING AND CURRENT ROW
                    return;
                }

                if (windowFrame.WindowFrameType == WindowFrameType.Range)
                {
                    sb.Append(" RANGE ");
                }
                else
                {
                    sb.Append(" ROWS ");
                }

                if (windowFrame.Bottom != null)
                {
                    sb.Append("BETWEEN ");
                }

                sb.Append(WindowDelimiterTypeToString(windowFrame.Top.WindowDelimiterType, windowFrame.Top.OffsetValue));

                if (windowFrame.Bottom != null)
                {
                    sb.Append(" AND ");
                    sb.Append(WindowDelimiterTypeToString(windowFrame.Bottom.WindowDelimiterType, windowFrame.Bottom.OffsetValue));
                }
            }

            private static string WindowDelimiterTypeToString(WindowDelimiterType delim, ScalarExpression offset)
            {
                switch (delim)
                {
                    case WindowDelimiterType.UnboundedPreceding:
                        return "UNBOUNDED PRECEDING";

                    case WindowDelimiterType.UnboundedFollowing:
                        return "UNBOUNDED FOLLOWING";

                    case WindowDelimiterType.CurrentRow:
                        return "CURRENT ROW";

                    case WindowDelimiterType.ValuePreceding:
                        return GetExpressionText(offset) + " PRECEDING";

                    case WindowDelimiterType.ValueFollowing:
                        return GetExpressionText(offset) + " FOLLOWING";

                    default:
                        return "";
                }
            }

            private static string GetExpressionText(ScalarExpression node)
            {
                if (node is ColumnReferenceExpression c)
                {
                    if (c.Collation is null)
                    {
                        return c.GetFullName();
                    }

                    return c.GetFullName() + " " + c.Collation.Value;
                }

                return node.GetFragmentCleanedText();
            }
        }
    }
}
