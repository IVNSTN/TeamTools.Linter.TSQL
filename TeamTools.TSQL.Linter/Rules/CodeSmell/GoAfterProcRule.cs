using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0732", "GO_AFTER_PROC")]
    internal sealed class GoAfterProcRule : AbstractRule
    {
        public GoAfterProcRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                ValidateProc(proc.StatementList);
            }
        }

        private void ValidateProc(StatementList node)
        {
            if ((node?.Statements?.Count ?? 0) <= 1)
            {
                // No body or single top-level statement (maybe BEGIN-END)
                return;
            }

            if (node.Statements[0] is BeginEndBlockStatement)
            {
                // If the first one is not BEGIN-END then it may be
                // a proc without top-level begin-end. Such cases are controlled
                // by separate rule.
                HandleNodeError(node.Statements[1]);
            }
        }
    }
}
