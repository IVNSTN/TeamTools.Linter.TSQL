using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Column reference visitor.
    /// </summary>
    internal partial class AmbiguousColumnBelongingRule
    {
        private class ColumnRefVisitor : TSqlFragmentVisitor
        {
            private static readonly ICollection<string> BuiltInFunctions
                = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            private static readonly ICollection<string> ReservedParamNames
                = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            private readonly List<ColumnReferenceExpression> ignored = new List<ColumnReferenceExpression>();

            static ColumnRefVisitor()
            {
                // TODO : consolidate all the metadata about known built-in functions
                BuiltInFunctions.Add("DATEADD");
                BuiltInFunctions.Add("DATEDIFF");
                BuiltInFunctions.Add("DATEPART");

                ReservedParamNames.Add("YEAR");
                ReservedParamNames.Add("YY");
                ReservedParamNames.Add("YYYY");

                ReservedParamNames.Add("QUARTER");
                ReservedParamNames.Add("QQ");
                ReservedParamNames.Add("Q");

                ReservedParamNames.Add("MONTH");
                ReservedParamNames.Add("MM");
                ReservedParamNames.Add("M");

                ReservedParamNames.Add("WEEK");
                ReservedParamNames.Add("WK");
                ReservedParamNames.Add("WW");

                ReservedParamNames.Add("DAY");
                ReservedParamNames.Add("DD");
                ReservedParamNames.Add("D");

                ReservedParamNames.Add("WEEKDAY");
                ReservedParamNames.Add("DW");

                ReservedParamNames.Add("DAYOFYEAR");
                ReservedParamNames.Add("DY");
                ReservedParamNames.Add("Y");

                ReservedParamNames.Add("HOUR");
                ReservedParamNames.Add("HH");

                ReservedParamNames.Add("MINUTE");
                ReservedParamNames.Add("MI");
                ReservedParamNames.Add("N");

                ReservedParamNames.Add("SECOND");
                ReservedParamNames.Add("SS");
                ReservedParamNames.Add("S");

                ReservedParamNames.Add("MILLISECOND");
                ReservedParamNames.Add("MS");
                ReservedParamNames.Add("MCS");

                ReservedParamNames.Add("NANOSECOND");
                ReservedParamNames.Add("NS");

                ReservedParamNames.Add("TZOFFSET");
                ReservedParamNames.Add("TZ");

                ReservedParamNames.Add("ISO_WEEK");
                ReservedParamNames.Add("ISOWK");
                ReservedParamNames.Add("ISOWW");
            }

            public List<MultiPartIdentifier> Columns { get; } = new List<MultiPartIdentifier>();

            public override void Visit(ColumnReferenceExpression node)
            {
                if (ignored.Contains(node) || node.MultiPartIdentifier is null)
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

                var fakeCols = node.Parameters
                    .OfType<ColumnReferenceExpression>()
                    .Where(col => col.MultiPartIdentifier.Identifiers.Count == 1)
                    .Where(col => ReservedParamNames.Contains(col.MultiPartIdentifier.Identifiers[0].Value));

                Ignore(fakeCols);
            }

            private void Ignore(IEnumerable<ColumnReferenceExpression> columns)
            {
                foreach (var col in columns)
                {
                    Ignore(col);
                }
            }

            private void Ignore(ColumnReferenceExpression column) => ignored.Add(column);
        }
    }
}
