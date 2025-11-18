using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0908", "NONCLUSTERED_IDX_INCLUDES_CLUSTERED")]
    [IndexRule]
    internal sealed class NonClusteredIndexIncludingClusteredColsRule : ScriptAnalysisServiceConsumingRule
    {
        public NonClusteredIndexIncludingClusteredColsRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var idxVisitor = GetService<TableIndicesVisitor>(node);

            if (idxVisitor.Indices.Count == 0)
            {
                return;
            }

            if (idxVisitor.Table.HasInMemoryFlag())
            {
                // ignoring memory-optimized tables since they have separate index architecture
                return;
            }

            var clusteredIndex = idxVisitor.Indices.FirstOrDefault(idx => idx.Clustered ?? false);
            if (clusteredIndex is null)
            {
                return;
            }

            var nonclusteredIndices = idxVisitor.Indices.Where(idx => !(idx.Clustered ?? false));
            if (!nonclusteredIndices.Any())
            {
                return;
            }

            ValidateIndexes(clusteredIndex, nonclusteredIndices);
        }

        private void ValidateIndexes(TableIndexInfo clusteredIndex, IEnumerable<TableIndexInfo> nonclusteredIndices)
        {
            var clusteredIndexCols = new List<string>(clusteredIndex.Columns.ExtractNames());

            foreach (var idx in nonclusteredIndices)
            {
                var nonclusteredIndexCols = new List<string>();
                var partitionedCols = new List<string>();
                // indexed columns
                nonclusteredIndexCols.AddRange(idx.Columns.ExtractNames());
                // included columns
                if (idx.Definition is IndexDefinition idxDef && idxDef.IncludeColumns?.Count > 0)
                {
                    nonclusteredIndexCols.AddRange(idxDef.IncludeColumns.ExtractNames());
                }
                else if (idx.Definition is CreateIndexStatement idxStmt)
                {
                    // included columns
                    if (idxStmt.IncludeColumns?.Count > 0)
                    {
                        nonclusteredIndexCols.AddRange(idxStmt.IncludeColumns.ExtractNames());
                    }

                    // partitioned on columns
                    if (idxStmt.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns.Count > 0)
                    {
                        partitionedCols.AddRange(idxStmt.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns.ExtractNames());
                    }
                }

                // if nonclustered index starts in different order then this is ok
                // but the rest of cols should not "reindex" clustered index cols
                // in any order; complicated optimization cases are not this rule problem
                // if you understand that the index is ok then just ignore the warning/hint
                if (!string.Equals(nonclusteredIndexCols[0], clusteredIndexCols[0], StringComparison.OrdinalIgnoreCase))
                {
                    nonclusteredIndexCols.RemoveAt(0);
                }

                // intersecting with columns from clustered index
                // except partitioning cols
                var illegalCols = nonclusteredIndexCols.Intersect(clusteredIndexCols).Except(partitionedCols);
                if (!illegalCols.Any())
                {
                    continue;
                }

                HandleNodeError(idx.Definition, string.Join(",", illegalCols.OrderBy(name => name)));
            }
        }
    }
}
