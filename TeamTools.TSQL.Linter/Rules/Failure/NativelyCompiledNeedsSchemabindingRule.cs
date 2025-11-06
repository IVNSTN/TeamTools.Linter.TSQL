using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Visit(ProcedureStatementBody node)
        {
            DetectViolation(
                node.Options,
                opt => opt.OptionKind == ProcedureOptionKind.NativeCompilation,
                opt => opt.OptionKind == ProcedureOptionKind.SchemaBinding);
        }

        public override void Visit(TriggerStatementBody node)
        {
            DetectViolation(
                node.Options,
                opt => opt.OptionKind == TriggerOptionKind.NativeCompile,
                opt => opt.OptionKind == TriggerOptionKind.SchemaBinding);
        }

        public override void Visit(FunctionStatementBody node)
        {
            DetectViolation(
                node.Options,
                opt => opt.OptionKind == FunctionOptionKind.NativeCompilation,
                opt => opt.OptionKind == FunctionOptionKind.SchemaBinding);
        }

        private void DetectViolation<T>(IList<T> options, Func<T, bool> findNativeCompilation, Func<T, bool> findSchemaBinding)
        where T : TSqlFragment
        {
            var native = options.FirstOrDefault(findNativeCompilation);

            if (native != null && !options.Any(findSchemaBinding))
            {
                HandleNodeError(native);
            }
        }
    }
}
