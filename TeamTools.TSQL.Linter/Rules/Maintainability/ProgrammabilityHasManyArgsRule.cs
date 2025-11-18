using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0818", "TOO_MANY_ARGS")]
    internal sealed class ProgrammabilityHasManyArgsRule : AbstractRule
    {
        private static readonly int MaxAllowedParameterCount = 12;

        public ProgrammabilityHasManyArgsRule() : base()
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

        private void DoValidate(ProcedureStatementBodyBase node)
        {
            int n = node.Parameters.Count;
            if (n > MaxAllowedParameterCount)
            {
                HandleNodeError(node.Parameters[n - 1], n.ToString());
            }
        }
    }
}
