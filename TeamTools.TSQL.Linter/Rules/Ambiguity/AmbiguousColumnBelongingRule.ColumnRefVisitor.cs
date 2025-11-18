using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Column reference visitor.
    /// </summary>
    internal partial class AmbiguousColumnBelongingRule
    {
        private sealed class ColumnRefVisitor : TSqlFragmentVisitor
        {
            private static readonly HashSet<string> BuiltInFunctions;

            private static readonly HashSet<string> ReservedParamNames;

            private List<ColumnReferenceExpression> ignored;

            static ColumnRefVisitor()
            {
                // TODO : consolidate all the metadata about known built-in functions
                BuiltInFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "DATEADD",
                    "DATEDIFF",
                    "DATEPART",
                };

                ReservedParamNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "YEAR",
                    "YY",
                    "YYYY",

                    "QUARTER",
                    "QQ",
                    "Q",

                    "MONTH",
                    "MM",
                    "M",

                    "WEEK",
                    "WK",
                    "WW",

                    "DAY",
                    "DD",
                    "D",

                    "WEEKDAY",
                    "DW",

                    "DAYOFYEAR",
                    "DY",
                    "Y",

                    "HOUR",
                    "HH",

                    "MINUTE",
                    "MI",
                    "N",

                    "SECOND",
                    "SS",
                    "S",

                    "MILLISECOND",
                    "MS",
                    "MCS",

                    "NANOSECOND",
                    "NS",

                    "TZOFFSET",
                    "TZ",

                    "ISO_WEEK",
                    "ISOWK",
                    "ISOWW",
                };
            }

            public List<MultiPartIdentifier> Columns { get; } = new List<MultiPartIdentifier>();

            public override void Visit(ColumnReferenceExpression node)
            {
                if (node.MultiPartIdentifier is null
                || (ignored != null && ignored.Contains(node)))
                {
                    return;
                }

                Columns.Add(node.MultiPartIdentifier);
            }

            public override void Visit(OutputIntoClause node) => Ignore(node.IntoTableColumns);

            public override void Visit(InsertSpecification node) => Ignore(node.Columns);

            public override void Visit(InsertMergeAction node) => Ignore(node.Columns);

            // In UPDATE SET all target columns belong to the target table
            public override void Visit(AssignmentSetClause node) => Ignore(node.Column);

            // ORDER BY may contain column aliases with no table alias
            // TODO : check if there are such aliases in the selected elements list
            public override void Visit(OrderByClause node)
            { }

            public override void Visit(FunctionCall node)
            {
                if (node.Parameters.Count == 0)
                {
                    return;
                }

                if (!BuiltInFunctions.Contains(node.FunctionName.Value))
                {
                    return;
                }

                int n = node.Parameters.Count;
                for (int i = 0; i < n; i++)
                {
                    if (node.Parameters[i] is ColumnReferenceExpression col
                    && col.MultiPartIdentifier.Identifiers.Count == 1
                    && ReservedParamNames.Contains(col.MultiPartIdentifier.Identifiers[0].Value))
                    {
                        Ignore(col);
                    }
                }
            }

            private void Ignore(IList<ColumnReferenceExpression> columns)
            {
                if (columns is null || columns.Count == 0)
                {
                    return;
                }

                (ignored ?? (ignored = new List<ColumnReferenceExpression>(columns.Count))).AddRange(columns);
            }

            private void Ignore(ColumnReferenceExpression column) => (ignored ?? (ignored = new List<ColumnReferenceExpression>())).Add(column);
        }
    }
}
