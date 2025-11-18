using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0774", "SPARSE_COL_INMEMORY")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [InMemoryRule]
    internal sealed class SparseColumnInMemoryRule : AbstractRule
    {
        public SparseColumnInMemoryRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.AsFileTable)
            {
                // filetable
                return;
            }

            if (!node.HasInMemoryFlag())
            {
                return;
            }

            HandleNodeErrorIfAny(DetectFirstSparseCol(node.Definition.ColumnDefinitions));
        }

        private static TSqlFragment DetectFirstSparseCol(IList<ColumnDefinition> cols)
        {
            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                var col = cols[i];
                if (col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None)
                {
                    return col;
                }
            }

            return default;
        }
    }
}
