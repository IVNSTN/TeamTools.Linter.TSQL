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
    internal sealed class NonClusteredIndexIncludingClusteredColsRule : AbstractRule
    {
        public NonClusteredIndexIncludingClusteredColsRule() : base()
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

            if (idxVisitor.Table.Options.Any(opt => opt.OptionKind == TableOptionKind.MemoryOptimized))
            {
                // ignoring memory-optimized tables since they have separate index architecture
                return;
            }

            var clusteredIndex = idxVisitor.Indices.FirstOrDefault(idx => idx.Clustered ?? false);
            if (clusteredIndex == null)
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
            var clusteredIndexCols = new List<string>();
            clusteredIndexCols.AddRange(clusteredIndex.Columns.Select(col => col.Column.MultiPartIdentifier.Identifiers.Last().Value));

            foreach (var idx in nonclusteredIndices)
            {
                var nonclusteredIndexCols = new List<string>();
                var partitionedCols = new List<string>();
                // indexed columns
                nonclusteredIndexCols.AddRange(idx.Columns.Select(col => col.Column.MultiPartIdentifier.Identifiers.Last().Value));
                // included columns
                if (idx.Definition is IndexDefinition idxDef && idxDef.IncludeColumns?.Count > 0)
                {
                    nonclusteredIndexCols.AddRange(idxDef.IncludeColumns.Select(col => col.MultiPartIdentifier.Identifiers.Last().Value));
                }
                else if (idx.Definition is CreateIndexStatement idxStmt)
                {
                    // included columns
                    if (idxStmt.IncludeColumns?.Count > 0)
                    {
                        nonclusteredIndexCols.AddRange(idxStmt.IncludeColumns.Select(col => col.MultiPartIdentifier.Identifiers.Last().Value));
                    }

                    // partitioned on columns
                    if (idxStmt.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns.Count > 0)
                    {
                        partitionedCols.AddRange(idxStmt.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns.Select(col => col.Value));
                    }
                }

                // if nonclustered index starts in different order then this is ok
                // but the rest of cols should not "reindex" clustered index cols
                // in any order; complicated optimization cases are not this rule problem
                // if you understand that the index is ok then just ignore the warning/hint
                if (!string.Equals(nonclusteredIndexCols.First(), clusteredIndexCols.First(), StringComparison.OrdinalIgnoreCase))
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
