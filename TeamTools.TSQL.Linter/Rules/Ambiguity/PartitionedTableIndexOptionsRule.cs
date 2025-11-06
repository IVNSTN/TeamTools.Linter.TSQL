using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : Clustered index definition may be separate from table definition
    [RuleIdentity("AM0129", "AMBIGUOUS_INDEX_PARTITIONING")]
    [IndexRule]
    internal sealed class PartitionedTableIndexOptionsRule : AbstractRule
    {
        public PartitionedTableIndexOptionsRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var tableVisitor = new TableIndicesVisitor();
            node.Accept(tableVisitor);

            if (tableVisitor.Table == null)
            {
                return;
            }

            if (tableVisitor.OnFileGroupOrPartitionScheme == null)
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
            foreach (TableIndexInfo idx in indices)
            {
                if (idx.OnFileGroupOrPartitionScheme != null)
                {
                    continue;
                }

                HandleNodeError(idx.Definition);
            }
        }
    }
}
