using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0274", "NUMBERED_SP")]
    internal sealed class NumberedSpRule : AbstractRule
    {
        public NumberedSpRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                HandleNodeErrorIfAny(proc.ProcedureReference.Number);
            }
        }
    }
}
