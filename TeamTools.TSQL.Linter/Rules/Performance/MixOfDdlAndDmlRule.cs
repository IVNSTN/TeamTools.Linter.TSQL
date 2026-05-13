using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0875", "DDL_DML_MIX")]
    internal sealed partial class MixOfDdlAndDmlRule : AbstractRule
    {
        public MixOfDdlAndDmlRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                proc.StatementList?.Accept(new DdlDmlMixVisitor(ViolationHandler));
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                trg.StatementList?.Accept(new DdlDmlMixVisitor(ViolationHandler));
            }
        }
    }
}
