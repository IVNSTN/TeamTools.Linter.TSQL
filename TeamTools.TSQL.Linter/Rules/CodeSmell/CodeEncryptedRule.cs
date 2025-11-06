using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0523", "CODE_ENCRYPTED")]
    internal sealed class CodeEncryptedRule : AbstractRule
    {
        public CodeEncryptedRule() : base()
        {
        }

        public override void Visit(ProcedureOption node)
        {
            if (node.OptionKind == ProcedureOptionKind.Encryption)
            {
                HandleNodeError(node);
            }
        }

        public override void Visit(TriggerOption node)
        {
            if (node.OptionKind == TriggerOptionKind.Encryption)
            {
                HandleNodeError(node);
            }
        }

        public override void Visit(FunctionOption node)
        {
            if (node.OptionKind == FunctionOptionKind.Encryption)
            {
                HandleNodeError(node);
            }
        }
    }
}
