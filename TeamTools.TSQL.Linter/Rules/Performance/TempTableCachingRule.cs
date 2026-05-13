using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0879", "TEMP_TABLE_CACHING_PREVENTED")]
    internal sealed partial class TempTableCachingRule : AbstractRule
    {
        public TempTableCachingRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                ValidateProc(proc);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                ValidateTrigger(trg);
            }
        }

        private void ValidateProc(ProcedureStatementBody node)
        {
            if ((node.StatementList?.Statements?.Count ?? 0) == 0)
            {
                // external, no body
                return;
            }

            var tempTableVisitor = MakeTempTableVisitor();
            node.StatementList.AcceptChildren(tempTableVisitor);

            if (tempTableVisitor.TempTables.Count > 0)
            {
                DetectBadProcOption(node.Options);
            }
        }

        private void ValidateTrigger(TriggerStatementBody node)
        {
            if ((node.StatementList?.Statements?.Count ?? 0) == 0)
            {
                // external, no body
                return;
            }

            var tempTableVisitor = MakeTempTableVisitor();
            node.StatementList.AcceptChildren(tempTableVisitor);
        }

        private TempTableVisitor MakeTempTableVisitor()
        {
            return new TempTableVisitor(ViolationHandlerWithMessage);
        }
    }
}
