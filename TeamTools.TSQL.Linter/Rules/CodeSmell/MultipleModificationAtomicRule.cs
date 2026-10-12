using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0469", "MULTI_DML_ATOMIC")]
    internal sealed partial class MultipleModificationAtomicRule : AbstractRule
    {
        public MultipleModificationAtomicRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            var code = GetBatchCode(batch);
            if (code is null)
            {
                return;
            }

            var blockAbortionDetector = new NoXactAbortDetector(ViolationHandlerWithMessage);
            code.AcceptChildren(blockAbortionDetector);

            var blockTransactionDetector = new DmlOutOfTranDetector(ViolationHandlerWithMessage);
            code.AcceptChildren(blockTransactionDetector);
        }

        private static TSqlFragment GetBatchCode(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                return GetModuleBody(proc.StatementList);
            }
            else if (firstStmt is TriggerStatementBody)
            {
                // triggers run in implicit transaction
                // XACT_ABORT is ON by default for then
                // and there is a separate rule for preventing switching it to OFF.
                // Considering every trigger to be in good state to avoid unnecessary false detections
                return default;
            }
            else if (firstStmt is FunctionStatementBody)
            {
                // functions are not supposed to edit data
                return default;
            }

            // ad-hoc script must be scanned for violation patterns
            return batch;
        }

        private static TSqlFragment GetModuleBody(StatementList body)
        {
            if (body is null || body.Statements.Count == 0)
            {
                // CLR or empty proc
                return default;
            }

            if (body.Statements[0] is BeginEndAtomicBlockStatement)
            {
                // in atomic XACT_ABORT is always ON and implicit transaction is active
                return default;
            }

            // regular body must be scanned for violation patterns
            return body;
        }
    }
}
