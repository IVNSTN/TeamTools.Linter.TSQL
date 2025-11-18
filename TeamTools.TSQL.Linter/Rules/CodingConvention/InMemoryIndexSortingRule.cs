using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0768", "INMEMORY_INDEX_SORTING")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [IndexRule]
    [InMemoryRule]
    internal sealed class InMemoryIndexSortingRule : AbstractRule
    {
        public InMemoryIndexSortingRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.AsFileTable)
            {
                return;
            }

            if (!node.HasInMemoryFlag())
            {
                return;
            }

            int n = node.Definition.Indexes.Count;
            for (int i = 0; i < n; i++)
            {
                var idx = node.Definition.Indexes[i];
                if (idx.IndexType.IndexTypeKind != IndexTypeKind.NonClustered)
                {
                    continue;
                }

                var unsortedCol = DetectColumnWithOutSortOrder(idx.Columns);
                HandleNodeErrorIfAny(unsortedCol, unsortedCol?.Column.MultiPartIdentifier.GetLastIdentifier().Value);
            }
        }

        private static ColumnWithSortOrder DetectColumnWithOutSortOrder(IList<ColumnWithSortOrder> columns)
        {
            int n = columns.Count;
            for (int i = 0; i < n; i++)
            {
                if (columns[i].SortOrder == SortOrder.NotSpecified)
                {
                    return columns[i];
                }
            }

            return default;
        }
    }
}
