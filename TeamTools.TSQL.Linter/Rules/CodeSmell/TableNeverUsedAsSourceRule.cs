using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0191", "NEVER_USED_SOURCE")]
    internal sealed class TableNeverUsedAsSourceRule : AbstractRule
    {
        public TableNeverUsedAsSourceRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                ValidateBatch(proc.StatementList);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                ValidateBatch(trg.StatementList);
            }
        }

        private void ValidateBatch(TSqlFragment node)
        {
            if (node is null)
            {
                return;
            }

            var tableDetector = new TableCreationDetector();
            node.AcceptChildren(tableDetector);

            if (tableDetector.Tables.Count == 0)
            {
                return;
            }

            var refDetector = new SourceTableReferenceDetector();
            node.AcceptChildren(refDetector);

            foreach (var tbl in tableDetector.Tables)
            {
                if (!refDetector.TableReferences.ContainsKey(tbl.Key))
                {
                    HandleNodeError(tbl.Value, tbl.Key);
                }
            }
        }
    }
}
