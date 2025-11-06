using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0907", "UNIQUE_INDEX_MISSING_PARTITIONED_COL")]
    [IndexRule]
    internal sealed class UniqueIndexMissingPartitionedColRule : AbstractRule
    {
        public UniqueIndexMissingPartitionedColRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var idxVisitor = new TableIndicesVisitor();
            node.Accept(idxVisitor);

            if (idxVisitor.Indices.Count == 0)
            {
                return;
            }

            List<string> tablePartitionedCols = new List<string>();

            if (idxVisitor.OnFileGroupOrPartitionScheme != null)
            {
                tablePartitionedCols.AddRange(idxVisitor.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.Select(col => col.Value).Distinct());
            }

            ValidateIndexes(idxVisitor, tablePartitionedCols);
        }

        private void ValidateIndexes(TableIndicesVisitor idxVisitor, List<string> tablePartitionedCols)
        {
            foreach (var idx in idxVisitor.Indices)
            {
                if (!idx.Unique)
                {
                    // only unique indices are troublesome
                    continue;
                }
                else if (idx.Clustered == true)
                {
                    // clustered can be on his own partition scheme or not partitioned
                    // TODO : is not this a bug?
                    continue;
                }
                else if (idx.OnFileGroupOrPartitionScheme != null && idx.OnFileGroupOrPartitionScheme.PartitionSchemeColumns == null)
                {
                    // if filegroup defined instead of partition schema then ok
                    continue;
                }
                else if (idx.OnFileGroupOrPartitionScheme == null && tablePartitionedCols.Count == 0)
                {
                    // if nothing defined and the table is not partitioned then also ok
                    continue;
                }

                List<string> idxPartitionedCols;

                if (idx.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns == null)
                {
                    idxPartitionedCols = tablePartitionedCols;
                }
                else
                {
                    idxPartitionedCols = new List<string>();
                    idxPartitionedCols.AddRange(idx.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.Select(col => col.Value).Distinct());
                }

                ValidateIndexedColumns(idx, idxPartitionedCols);
            }
        }

        private void ValidateIndexedColumns(TableIndexInfo idx, IList<string> idxPartitionedCols)
        {
            var indexKeyCols = idx.Columns
                .Select(idxCol => idxCol.Column.MultiPartIdentifier.Identifiers.Last().Value)
                .Distinct()
                .ToList();

            var missingPartitionedCols = idxPartitionedCols
                .Where(prtCol => !indexKeyCols.Contains(prtCol, StringComparer.OrdinalIgnoreCase))
                .Distinct()
                .ToList();

            if (missingPartitionedCols.Count == 0)
            {
                return;
            }

            HandleNodeError(idx.Definition, string.Join(",", missingPartitionedCols));
        }
    }
}
