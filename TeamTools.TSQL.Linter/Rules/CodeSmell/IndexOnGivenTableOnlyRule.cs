using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0128", "INDEX_MISDIRECTED")]
    [IndexRule]
    internal sealed class IndexOnGivenTableOnlyRule : ScriptAnalysisServiceConsumingRule
    {
        public IndexOnGivenTableOnlyRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var tableVisitor = GetService<TableIndicesVisitor>(node);

            if (tableVisitor.Table is null)
            {
                return;
            }

            CheckTableIndices(tableVisitor.Table.SchemaObjectName, tableVisitor.Indices);
        }

        private void CheckTableIndices(SchemaObjectName table, IList<TableIndexInfo> indices)
        {
            int n = indices.Count;
            for (int i = 0; i < n; i++)
            {
                var idx = indices[i];

                // inline index definitions
                if (idx.OnName is null)
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
