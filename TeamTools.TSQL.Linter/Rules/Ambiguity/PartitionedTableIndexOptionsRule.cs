using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : Clustered index definition may be separate from table definition
    [RuleIdentity("AM0129", "AMBIGUOUS_INDEX_PARTITIONING")]
    [IndexRule]
    internal sealed class PartitionedTableIndexOptionsRule : ScriptAnalysisServiceConsumingRule
    {
        public PartitionedTableIndexOptionsRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var tableVisitor = GetService<TableIndicesVisitor>(node);

            if (tableVisitor.Table is null || tableVisitor.OnFileGroupOrPartitionScheme is null)
            {
                return;
            }

            // fg is fine, only partitioning is questionable
            if (tableVisitor.OnFileGroupOrPartitionScheme.PartitionSchemeColumns.Count == 0)
            {
                return;
            }

            ValidateTableIndices(tableVisitor.Table.SchemaObjectName, tableVisitor.Indices);
        }

        private void ValidateTableIndices(SchemaObjectName table, IList<TableIndexInfo> indices)
        {
            int n = indices.Count;
            for (int i = 0; i < n; i++)
            {
                var idx = indices[i];

                if (idx.OnFileGroupOrPartitionScheme is null)
                {
                    HandleNodeError(idx.Definition);
                }
            }
        }
    }
}
