using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0928", "FILTERED_IDX_FOR_NULL_COL_NOT_INCLUDED")]
    [IndexRule]
    internal sealed class IndexFilterColumnNullMissingInKeyRule : AbstractRule
    {
        private static readonly string ColListSeparator = ", ";

        public IndexFilterColumnNullMissingInKeyRule() : base()
        {
        }

        public override void Visit(IndexDefinition node)
        {
            ValidateFilteredColumns(node.FilterPredicate, node.Columns, node.IncludeColumns);
        }

        public override void Visit(CreateIndexStatement node)
        {
            ValidateFilteredColumns(node.FilterPredicate, node.Columns, node.IncludeColumns);
        }

        private static ICollection<string> GetFilteredForNullColumns(BooleanExpression predicate)
        {
            var colFilters = new List<ColumnReferenceExpression>();
            ExtractIsNullPredicates(predicate, ref colFilters);
            if (colFilters.Count == 0)
            {
                return Enumerable.Empty<string>().ToArray();
            }

            return new HashSet<string>(colFilters.ExtractNames(), StringComparer.OrdinalIgnoreCase);
        }

        private static void ExtractIsNullPredicates(BooleanExpression predicate, ref List<ColumnReferenceExpression> colRefs)
        {
            while (predicate is BooleanParenthesisExpression pe)
            {
                predicate = pe.Expression;
            }

            if (predicate is BooleanBinaryExpression bin)
            {
                ExtractIsNullPredicates(bin.FirstExpression, ref colRefs);
                ExtractIsNullPredicates(bin.SecondExpression, ref colRefs);
            }
            else if (predicate is BooleanIsNullExpression isnull && !isnull.IsNot)
            {
                var colRef = ExtractColumnReference(isnull.Expression);
                if (colRef != null)
                {
                    colRefs.Add(colRef);
                }
            }
        }

        private static ColumnReferenceExpression ExtractColumnReference(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is UnaryExpression un)
            {
                return ExtractColumnReference(un.Expression);
            }

            if (expr is ColumnReferenceExpression col)
            {
                return col;
            }

            return null;
        }

        private static void ExcludeCols(ICollection<string> colList, IEnumerable<ColumnReferenceExpression> excludedCols)
        {
            if (excludedCols is null)
            {
                return;
            }

            foreach (var col in excludedCols)
            {
                colList.Remove(col.MultiPartIdentifier.GetLastIdentifier().Value);
            }
        }

        private void ValidateFilteredColumns(BooleanExpression filterPredicate, IList<ColumnWithSortOrder> indexedCols, IList<ColumnReferenceExpression> includedCols)
        {
            if (filterPredicate is null)
            {
                return;
            }

            var filteredColumnNames = GetFilteredForNullColumns(filterPredicate);

            if (filteredColumnNames.Count == 0)
            {
                return;
            }

            ExcludeCols(filteredColumnNames, indexedCols.Select(sortedCol => sortedCol.Column));
            ExcludeCols(filteredColumnNames, includedCols);

            if (filteredColumnNames.Count == 0)
            {
                return;
            }

            HandleNodeError(filterPredicate, string.Join(ColListSeparator, filteredColumnNames));
        }
    }
}
