using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0831", "HISTORY_IN_SAME_SCHEMA")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class TemporalTableSameSchemaRule : AbstractRule
    {
        public TemporalTableSameSchemaRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            var schemaName = node.SchemaObjectName.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;

            for (int i = 0, n = node.Options.Count; i < n; i++)
            {
                if (node.Options[i] is SystemVersioningTableOption history)
                {
                    var historySchema = history.HistoryTable.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;

                    if (!string.Equals(schemaName, historySchema, StringComparison.OrdinalIgnoreCase))
                    {
                        HandleNodeError(history.HistoryTable, $"{historySchema} != {schemaName}");
                    }
                }
            }
        }
    }
}
