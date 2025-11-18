using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0763", "NATIVE_COMPILATION_SCHEMABINDING")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    [InMemoryRule]
    internal sealed class NativelyCompiledNeedsSchemabindingRule : AbstractRule
    {
        public NativelyCompiledNeedsSchemabindingRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DetectViolation(
                    proc.Options,
                    opt => opt.OptionKind == ProcedureOptionKind.NativeCompilation,
                    opt => opt.OptionKind == ProcedureOptionKind.SchemaBinding);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                DetectViolation(
                    trg.Options,
                    opt => opt.OptionKind == TriggerOptionKind.NativeCompile,
                    opt => opt.OptionKind == TriggerOptionKind.SchemaBinding);
            }
            else if (firstStmt is FunctionStatementBody fn)
            {
                DetectViolation(
                    fn.Options,
                    opt => opt.OptionKind == FunctionOptionKind.NativeCompilation,
                    opt => opt.OptionKind == FunctionOptionKind.SchemaBinding);
            }
        }

        private static bool IsMatch<T>(IList<T> options, Predicate<T> condition, out T location)
        where T : TSqlFragment
        {
            location = default;
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                var opt = options[i];
                if (condition(opt))
                {
                    location = opt;
                    return true;
                }
            }

            return false;
        }

        private void DetectViolation<T>(IList<T> options, Predicate<T> findNativeCompilation, Predicate<T> findSchemaBinding)
        where T : TSqlFragment
        {
            var isNative = IsMatch(options, findNativeCompilation, out var nativeCompilationOption);

            if (isNative && !IsMatch(options, findSchemaBinding, out _))
            {
                HandleNodeError(nativeCompilationOption);
            }
        }
    }
}
