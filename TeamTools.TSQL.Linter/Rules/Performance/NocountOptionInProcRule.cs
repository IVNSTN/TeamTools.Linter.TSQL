using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0144", "PROC_SET_NOCOUNT")]
    internal sealed class NocountOptionInProcRule : AbstractRule
    {
        public NocountOptionInProcRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DoValidate(proc);
            }
        }

        private void DoValidate(ProcedureStatementBody node)
        {
            if (node.StatementList is null || node.StatementList.Statements.Count == 0)
            {
                // CLR or empty proc
                return;
            }

            if (node.StatementList.Statements[0] is BeginEndAtomicBlockStatement)
            {
                // in atomic NOCOUNT is always ON
                return;
            }

            var nocountVisitor = new SetOptionsVisitor();
            node.StatementList.AcceptChildren(nocountVisitor);

            // Nocount was set to ON
            if (nocountVisitor.DetectedOptions.TryGetValue(SetOptions.NoCount, out var nocountState)
            && (nocountState ?? false))
            {
                return;
            }

            HandleNodeError(node.StatementList.GetFirstStatement());
        }
    }
}
