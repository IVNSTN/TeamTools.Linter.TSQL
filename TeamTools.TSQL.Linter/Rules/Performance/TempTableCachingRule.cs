using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

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
                ValidateBody(proc.StatementList, proc.Options);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                ValidateBody(trg.StatementList);
            }
        }

        private void ValidateBody(StatementList body, IList<ProcedureOption> options = null)
        {
            if ((body?.Statements?.Count ?? 0) == 0)
            {
                // external, no body
                return;
            }

            var tempTableVisitor = MakeTempTableVisitor();
            body.AcceptChildren(tempTableVisitor);

            if (options != null && tempTableVisitor.OperatesWithTempTables)
            {
                DetectBadProcOption(options);
            }
        }

        private TempTableVisitor MakeTempTableVisitor()
        {
            return new TempTableVisitor(ViolationHandlerWithMessage);
        }
    }
}
