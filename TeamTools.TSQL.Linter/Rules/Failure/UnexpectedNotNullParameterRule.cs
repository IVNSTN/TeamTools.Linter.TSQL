using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0918", "INVALID_NOT_NULL_PARAM")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    public sealed class UnexpectedNotNullParameterRule : AbstractRule
    {
        public UnexpectedNotNullParameterRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DoValidate(proc);
            }
            else if (firstStmt is FunctionStatementBody fn)
            {
                DoValidate(fn);
            }
        }

        private void DoValidate(ProcedureStatementBody node)
        {
            if (node.Options.HasOption(ProcedureOptionKind.NativeCompilation))
            {
                // NOT NULL params are allowed in Natively compiled procs
                return;
            }

            ValidateParamsNullability(node.Parameters);
        }

        private void DoValidate(FunctionStatementBody node)
        {
            if (node.ReturnType is SelectFunctionReturnType)
            {
                // NOT NULL params are allowed in inlined table-valued functions
                return;
            }

            ValidateParamsNullability(node.Parameters);
        }

        private void ValidateParamsNullability(IList<ProcedureParameter> parameters)
        {
            if (parameters.Count == 0)
            {
                return;
            }

            for (int i = 0, n = parameters.Count; i < n; i++)
            {
                var prm = parameters[i];
                if (prm.Nullable?.Nullable == false)
                {
                    HandleNodeError(prm.Nullable, prm.VariableName.Value);
                }
            }
        }
    }
}
