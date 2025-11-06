using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0769", "SPARSE_CANNOT_BE_KEY")]
    internal sealed class SparseCannotBeKeyRule : AbstractRule
    {
        public SparseCannotBeKeyRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var extractor = new TableDefinitionElementsEnumerator(node);
            var tables = extractor.Tables;

            foreach (var tbl in tables.Keys)
            {
                var sparseCols = tables[tbl].Columns
                    .Where(col => col.Value.IsSparse)
                    .Select(col => col.Value.Name)
                    .ToList();

                if (!sparseCols.Any())
                {
                    continue;
                }

                // PK and anything CLUSTERED
                var keys = extractor.Keys(tbl)
                    .Where(k => k.ElementType == SqlTableElementType.PrimaryKey)
                    .OfType<SqlIndexInfo>()
                    .Union(extractor.Indices(tbl).OfType<SqlIndexInfo>().Where(idx => idx.IsClustered))
                    .Distinct();

                foreach (var key in keys)
                {
                    key.Columns
                        .Where(col => sparseCols.Contains(col.Name, StringComparer.OrdinalIgnoreCase))
                        .ToList()
                        .ForEach(keyCol => HandleNodeError(keyCol.Reference, keyCol.Name));
                }
            }
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.OnFileGroupOrPartitionScheme is null)
            {
                return;
            }

            var sparseCols = node.Definition.ColumnDefinitions
                .Where(col => col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None)
                .Select(col => col.ColumnIdentifier.Value)
                .ToList();

            if (!sparseCols.Any())
            {
                return;
            }

            node.OnFileGroupOrPartitionScheme.PartitionSchemeColumns
                .Where(col => sparseCols.Contains(col.Value, StringComparer.OrdinalIgnoreCase))
                .ToList()
                .ForEach(col => HandleNodeError(col, col.Value));
        }
    }
}
