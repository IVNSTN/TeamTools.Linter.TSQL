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
    internal sealed class UniqueIndexMissingPartitionedColRule : ScriptAnalysisServiceConsumingRule
    {
        public UniqueIndexMissingPartitionedColRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var idxVisitor = GetService<TableIndicesVisitor>(node);

            if (idxVisitor.Indices.Count == 0)
            {
                return;
            }

            List<string> tablePartitionedCols = null;

            if (idxVisitor.OnFileGroupOrPartitionScheme != null)
            {
                tablePartitionedCols = new List<string>(idxVisitor.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.ExtractNames().Distinct());
            }

            ValidateIndexes(idxVisitor, tablePartitionedCols);
        }

        // TODO : optimization needed
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
                else if (idx.OnFileGroupOrPartitionScheme != null && idx.OnFileGroupOrPartitionScheme.PartitionSchemeColumns is null)
                {
                    // if filegroup defined instead of partition schema then ok
                    continue;
                }
                else if (idx.OnFileGroupOrPartitionScheme is null && (tablePartitionedCols is null || tablePartitionedCols.Count == 0))
                {
                    // if nothing defined and the table is not partitioned then also ok
                    continue;
                }

                List<string> idxPartitionedCols;

                if (idx.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns is null)
                {
                    idxPartitionedCols = tablePartitionedCols;
                }
                else
                {
                    idxPartitionedCols = new List<string>(idx.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.ExtractNames().Distinct());
                }

                ValidateIndexedColumns(idx, idxPartitionedCols);
            }
        }

        // TODO : less linq, less string manufacturing
        private void ValidateIndexedColumns(TableIndexInfo idx, List<string> idxPartitionedCols)
        {
            if (idxPartitionedCols is null || idxPartitionedCols.Count == 0)
            {
                return;
            }

            var indexKeyCols = new HashSet<string>(idx.Columns.ExtractNames(), StringComparer.OrdinalIgnoreCase);

            for (int i = 0, n = idxPartitionedCols.Count; i < n; i++)
            {
                var prtCol = idxPartitionedCols[i];

                if (!indexKeyCols.Contains(prtCol))
                {
                    HandleNodeError(idx.Definition, prtCol);
                    // TODO : report all?
                    return;
                }
            }
        }
    }
}
