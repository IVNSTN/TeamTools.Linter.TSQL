using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0769", "SPARSE_CANNOT_BE_KEY")]
    [IndexRule]
    internal sealed class SparseCannotBeKeyRule : ScriptAnalysisServiceConsumingRule
    {
        public SparseCannotBeKeyRule() : base()
        {
        }

        // TODO : avoid double-scan, take all info from TableDefinitionElementsEnumerator
        public override void ExplicitVisit(CreateTableStatement node)
        {
            if (node.OnFileGroupOrPartitionScheme is null)
            {
                return;
            }

            var sparseCols = new HashSet<string>(
                node.Definition.ColumnDefinitions
                    .Where(col => col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None)
                    .Select(col => col.ColumnIdentifier.Value),
                StringComparer.OrdinalIgnoreCase);

            if (sparseCols.Count == 0)
            {
                return;
            }

            int n = node.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.Count;
            for (int i = 0; i < n; i++)
            {
                var col = node.OnFileGroupOrPartitionScheme.PartitionSchemeColumns[i];
                var colName = col.Value;
                if (sparseCols.Contains(colName))
                {
                    HandleNodeError(col, colName);
                }
            }
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var extractor = GetService<TableDefinitionElementsEnumerator>(node);
            var tables = extractor.Tables;

            if (extractor.Tables.Count == 0)
            {
                return;
            }

            foreach (var tbl in tables.Keys)
            {
                var sparseCols = new HashSet<string>(ListSparseCols(tables[tbl].Columns), StringComparer.OrdinalIgnoreCase);

                if (sparseCols.Count == 0)
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
                        .Where(col => sparseCols.Contains(col.Name))
                        .ToList()
                        .ForEach(keyCol => HandleNodeError(keyCol.Reference, keyCol.Name));
                }
            }

            // to catch partitioning info
            node.Accept(this);
        }

        private static IEnumerable<string> ListSparseCols(IDictionary<string, SqlColumnInfo> cols)
        {
            foreach (var col in cols)
            {
                if (col.Value.IsSparse)
                {
                    yield return col.Key;
                }
            }
        }
    }
}
