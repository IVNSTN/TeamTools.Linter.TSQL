using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0465", "REDUNDANT_OPTION_CLEANUP")]
    internal sealed partial class RedundantOptionCleanupRule : AbstractRule
    {
        public RedundantOptionCleanupRule() : base()
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

            var visitor = new OptionCleanupVisitor();
            body.Accept(visitor);
            HandleNodeErrorIfAny(visitor.ViolationNode);
        }
    }
}
