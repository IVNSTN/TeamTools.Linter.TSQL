using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CD0717", "CREATE_OR_ALTER")]
    internal sealed class CreateOrAlterRule : AbstractRule
    {
        public CreateOrAlterRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE OR ALTER func/proc/trg/view must be the first statement in a batch
            var firstStmt = batch.Statements[0];

            if (firstStmt is CreateOrAlterFunctionStatement
            || firstStmt is CreateOrAlterProcedureStatement
            || firstStmt is CreateOrAlterTriggerStatement
            || firstStmt is CreateOrAlterViewStatement)
            {
                HandleNodeError(firstStmt);
            }
        }
    }
}
