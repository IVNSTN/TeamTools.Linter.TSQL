using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0464", "REDUNDANT_TABLE_CLEANUP")]
    internal sealed partial class RedundantTableCleanupRule : AbstractRule
    {
        public RedundantTableCleanupRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                ValidateBody(proc.StatementList);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                ValidateBody(trg.StatementList);
            }
        }

        private void ValidateBody(StatementList body)
        {
            if (body is null)
            {
                // CLR module
                return;
            }

            var visitor = new TableCleanupVisitor();
            body.Accept(visitor);
            HandleNodeErrorIfAny(visitor.ViolationNode);
        }
    }
}
