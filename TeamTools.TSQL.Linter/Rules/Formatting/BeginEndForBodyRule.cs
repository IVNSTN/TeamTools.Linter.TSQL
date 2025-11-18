using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FM0120", "BODY_BEGIN_END")]
    internal sealed class BeginEndForBodyRule : AbstractRule
    {
        public BeginEndForBodyRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DoValidateBody(proc.StatementList);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                DoValidateBody(trg.StatementList);
            }
        }

        private void DoValidateBody(StatementList body)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // CLR / external
                return;
            }

            var firstStmt = body.Statements[0];

            if (firstStmt is BeginEndBlockStatement)
            {
                return;
            }

            HandleNodeError(firstStmt);
        }
    }
}
