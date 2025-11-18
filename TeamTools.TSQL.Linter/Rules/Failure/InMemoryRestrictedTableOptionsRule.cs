using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

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
            if (!node.HasInMemoryFlag())
            {
                return;
            }

            HandleNodeErrorIfAny(node.OnFileGroupOrPartitionScheme, "table placement");
            HandleNodeErrorIfAny(node.FileStreamOn, "FILESTREAM");
            HandleNodeErrorIfAny(DetectDataCompressionOption(node.Options), "DATACOMPRESSION");

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
            if (!node.HasInMemoryFlag())
            {
                return;
            }

            ValidateColumns(node.Definition.ColumnDefinitions);
        }

        private static TableOption DetectDataCompressionOption(IList<TableOption> options)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i].OptionKind == TableOptionKind.DataCompression)
                {
                    return options[i];
                }
            }

            return default;
        }

        private void ValidateColumns(IList<ColumnDefinition> cols)
        {
            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                var col = cols[i];
                if (col.IsRowGuidCol)
                {
                    HandleNodeError(col, "ROWGUIDCOL");
                }

                if (col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None)
                {
                    HandleNodeError(col, "SPARSE");
                }
            }
        }
    }
}
