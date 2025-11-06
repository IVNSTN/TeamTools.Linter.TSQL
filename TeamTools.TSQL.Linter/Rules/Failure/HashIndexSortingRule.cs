using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
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

        private void ValidateIndexColumns(IndexType indexType, IList<ColumnWithSortOrder> columns)
        {
            if (indexType?.IndexTypeKind != IndexTypeKind.NonClusteredHash)
            {
                return;
            }

            var sortedCol = columns.FirstOrDefault(col => col.SortOrder != SortOrder.NotSpecified);
            HandleNodeErrorIfAny(sortedCol, sortedCol?.SortOrder.ToString().ToUpperInvariant());
        }
    }
}
