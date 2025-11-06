using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0762", "INMEMORY_RESTRICTED_TABLE_OPTIONS")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [InMemoryRule]
    internal sealed class InMemoryRestrictedTableOptionsRule : AbstractRule
    {
        public InMemoryRestrictedTableOptionsRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            if (!node.Options.Any(opt => opt.OptionKind == TableOptionKind.MemoryOptimized))
            {
                return;
            }

            HandleNodeErrorIfAny(node.OnFileGroupOrPartitionScheme, "table placement");
            HandleNodeErrorIfAny(node.FileStreamOn, "FILESTREAM");
            HandleNodeErrorIfAny(node.Options.FirstOrDefault(opt => opt.OptionKind == TableOptionKind.DataCompression), "DATACOMPRESSION");

            if (node.AsFileTable)
            {
                HandleNodeError(node, "FILETABLE");
            }

            if (node.Definition is null)
            {
                // filestream
                return;
            }

            ValidateColumns(node.Definition.ColumnDefinitions);
        }

        public override void Visit(CreateTypeTableStatement node)
        {
            if (!node.Options.Any(opt => opt.OptionKind == TableOptionKind.MemoryOptimized))
            {
                return;
            }

            ValidateColumns(node.Definition.ColumnDefinitions);
        }

        private void ValidateColumns(IList<ColumnDefinition> cols)
        {
            cols
                .Where(col => col.IsRowGuidCol)
                .ToList()
                .ForEach(col => HandleNodeError(col, "ROWGUIDCOL"));

            cols
                .Where(col => col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None)
                .ToList()
                .ForEach(col => HandleNodeError(col, "SPARSE"));
        }
    }
}
