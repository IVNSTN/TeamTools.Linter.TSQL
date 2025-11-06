using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

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

        public override void Visit(ProcedureStatementBody node)
        {
            if (!node.Options.Any(opt => opt.OptionKind == ProcedureOptionKind.NativeCompilation))
            {
                return;
            }

            node.Parameters
                .Where(p => p.Nullable is null && p.Modifier != ParameterModifier.ReadOnly) // READONLY means this is a table type
                .ToList()
                .ForEach(p => HandleNodeError(p, p.VariableName.Value));
        }
    }
}
