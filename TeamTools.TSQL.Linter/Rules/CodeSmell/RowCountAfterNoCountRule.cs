using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0126", "BAD_ROWCOUNT_CHECK")]
    internal sealed partial class RowCountAfterNoCountRule : AbstractRule
    {
        public RowCountAfterNoCountRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            bool rowCountInitialized = false;

            // CREATE PROC/TRIGGER/FUNC would be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                if (proc.StatementList is null
                || (proc.Options != null && proc.Options.HasOption(ProcedureOptionKind.NativeCompilation)))
                {
                    // natively compiled procs don't change ROWCOUNT after DML
                    return;
                }
            }
            else if (firstStmt is TriggerStatementBody)
            {
                rowCountInitialized = true;
            }

            batch.AcceptChildren(new RowCountChangerVisitor(rowCountInitialized, ViolationHandler));
        }
    }
}
