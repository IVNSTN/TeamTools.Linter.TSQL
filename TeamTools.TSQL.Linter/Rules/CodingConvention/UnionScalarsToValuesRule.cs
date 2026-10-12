using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0237", "UNION_SCALAR_TO_VALUES")]
    internal sealed partial class UnionScalarsToValuesRule : AbstractRule
    {
        public UnionScalarsToValuesRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            if (IsNativelyCompiledModule(batch))
            {
                // Multi-row VALUES clause is not supported in natively compiled objects.
                return;
            }

            batch.Accept(new CollapsibleSelectDetector(ViolationHandler));
        }

        private static bool IsNativelyCompiledModule(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch.
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                return proc.Options.HasOption(ProcedureOptionKind.NativeCompilation);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                return trg.Options.HasOption(TriggerOptionKind.NativeCompile);
            }
            else if (firstStmt is FunctionStatementBody fn)
            {
                return fn.Options.HasOption(FunctionOptionKind.NativeCompilation);
            }

            return false;
        }
    }
}
