using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0128", "INDEX_MISDIRECTED")]
    [IndexRule]
    internal sealed class IndexOnGivenTableOnlyRule : AbstractRule
    {
        public IndexOnGivenTableOnlyRule() : base()
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

            CheckTableIndices(tableVisitor.Table.SchemaObjectName, tableVisitor.Indices);
        }

        private void CheckTableIndices(SchemaObjectName table, IList<TableIndexInfo> indices)
        {
            foreach (TableIndexInfo idx in indices)
            {
                // inline index definitions
                if (idx.OnName == null)
                {
                    continue;
                }

                string tableSchema = table.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;
                string indexSchema = idx.OnName.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;

                if (indexSchema.Equals(tableSchema, StringComparison.OrdinalIgnoreCase))
                {
                    if (idx.OnName.BaseIdentifier.Value.Equals(table.BaseIdentifier.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                string realTargetTableName = string.Concat(indexSchema, TSqlDomainAttributes.NamePartSeparator, idx.OnName.BaseIdentifier.Value);

                HandleNodeError(idx.OnName, realTargetTableName);
            }
        }
    }
}
