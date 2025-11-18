using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0975", "TRIGGER_SCHEMA_TABLE_SCHEMA")]
    [TriggerRule]
    internal sealed class TriggerSchemaFromTableRule : AbstractRule
    {
        public TriggerSchemaFromTableRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is TriggerStatementBody trg)
            {
                DoValidate(trg);
            }
        }

        private void DoValidate(TriggerStatementBody node)
        {
            if (node.TriggerObject.TriggerScope != TriggerScope.Normal)
            {
                // database- and server-level triggers are not linked to tables
                return;
            }

            string triggerSchema = node.Name.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;
            string tableSchema = node.TriggerObject.Name.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;

            if (string.Equals(triggerSchema, tableSchema, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            HandleNodeError(node.TriggerObject.Name, $"{triggerSchema} vs {tableSchema}");
        }
    }
}
