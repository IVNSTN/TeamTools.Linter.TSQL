using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0817", "ZERO_ARGS")]
    internal sealed class ProgrammabilityHasZeroArgsRule : AbstractRule
    {
        public ProgrammabilityHasZeroArgsRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBodyBase procOrFunc)
            {
                DoValidate(procOrFunc);
            }
        }

        // This will catch both functions and procedures
        private void DoValidate(ProcedureStatementBodyBase node)
        {
            if (node.Parameters.Count > 0)
            {
                return;
            }

            TSqlFragment reportedNode = node;
            if (node is FunctionStatementBody fn)
            {
                reportedNode = fn.Name;
            }
            else if (node is ProcedureStatementBody proc)
            {
                reportedNode = proc.ProcedureReference;
            }

            HandleNodeError(reportedNode);
        }
    }
}
