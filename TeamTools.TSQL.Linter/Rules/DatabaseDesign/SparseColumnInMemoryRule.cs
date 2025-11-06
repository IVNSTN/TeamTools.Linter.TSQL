using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0774", "SPARSE_COL_INMEMORY")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class SparseColumnInMemoryRule : AbstractRule
    {
        public SparseColumnInMemoryRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            if (!node.Options.Any(opt => opt.OptionKind == TableOptionKind.MemoryOptimized))
            {
                return;
            }

            if ((node.Definition?.ColumnDefinitions?.Count ?? 0) == 0)
            {
                // filetable
                return;
            }

            var sparseCol = node.Definition.ColumnDefinitions
                .FirstOrDefault(col => col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None);

            HandleNodeErrorIfAny(sparseCol);
        }
    }
}
