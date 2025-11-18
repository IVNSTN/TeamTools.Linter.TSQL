using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0764", "HASH_INDEX_SORTING")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [IndexRule]
    [InMemoryRule]
    internal sealed class HashIndexSortingRule : AbstractRule
    {
        public HashIndexSortingRule() : base()
        {
        }

        public override void Visit(UniqueConstraintDefinition node) => ValidateIndexColumns(node.IndexType, node.Columns);

        public override void Visit(IndexDefinition node) => ValidateIndexColumns(node.IndexType, node.Columns);

        private static ColumnWithSortOrder DetectColumnWithExplicitSortOrder(IList<ColumnWithSortOrder> columns)
        {
            int n = columns.Count;
            for (int i = 0; i < n; i++)
            {
                var col = columns[i];
                if (col.SortOrder != SortOrder.NotSpecified)
                {
                    return col;
                }
            }

            return default;
        }

        private static string SortOrderToString(SortOrder sortOrder) => sortOrder == SortOrder.Ascending ? "ASC" : "DESC";

        private void ValidateIndexColumns(IndexType indexType, IList<ColumnWithSortOrder> columns)
        {
            if (indexType?.IndexTypeKind != IndexTypeKind.NonClusteredHash)
            {
                return;
            }

            var sortedCol = DetectColumnWithExplicitSortOrder(columns);
            if (sortedCol is null)
            {
                return;
            }

            HandleNodeError(sortedCol, SortOrderToString(sortedCol.SortOrder));
        }
    }
}
