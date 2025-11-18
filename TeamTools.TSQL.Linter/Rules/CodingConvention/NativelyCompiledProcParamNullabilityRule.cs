using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0767", "NATIVELY_PARAM_NULLABILITY")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [InMemoryRule]
    internal sealed class NativelyCompiledProcParamNullabilityRule : AbstractRule
    {
        public NativelyCompiledProcParamNullabilityRule() : base()
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
        }

        private static bool IsNullabilityUndefined(ProcedureParameter prm)
            // READONLY means this is a table type and it should be ignored here
            => prm.Nullable is null && prm.Modifier != ParameterModifier.ReadOnly;

        private void DoValidate(ProcedureStatementBody node)
        {
            if (!node.Options.HasOption(ProcedureOptionKind.NativeCompilation))
            {
                return;
            }

            int n = node.Parameters.Count;
            for (int i = 0; i < n; i++)
            {
                var p = node.Parameters[i];
                if (IsNullabilityUndefined(p))
                {
                    HandleNodeError(p, p.VariableName.Value);
                }
            }
        }
    }
}
