using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0765", "NATIVELY_COMPILED_TRIGGER")]
    [InMemoryRule]
    internal sealed class NativelyCompiledTriggerRule : AbstractRule
    {
        public NativelyCompiledTriggerRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node)
        {
            HandleNodeErrorIfAny(node.Options.FirstOrDefault(opt => opt.OptionKind == TriggerOptionKind.NativeCompile));
        }
    }
}
