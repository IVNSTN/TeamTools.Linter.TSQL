using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0711", "DDL_TRIGGER_NO_SCHEMA")]
    [TriggerRule]
    internal sealed class DdlTriggerSchemaRule : AbstractRule
    {
        public DdlTriggerSchemaRule() : base()
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
            if (node.TriggerObject.TriggerScope == TriggerScope.Normal)
            {
                // DML-triggers can have schema
                return;
            }

            HandleNodeErrorIfAny(node.Name.SchemaIdentifier);
        }
    }
}
