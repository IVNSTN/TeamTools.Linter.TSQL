using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0145", "TRIGGER_SET_NOCOUNT")]
    [TriggerRule]
    internal sealed class NocountOptionInTriggerRule : AbstractRule
    {
        public NocountOptionInTriggerRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node)
        {
            if (node.Options.Any(opt => opt.OptionKind == TriggerOptionKind.NativeCompile))
            {
                // natively compiled triggers cannot have SET NOCOUNT clause
                return;
            }

            var nocountVisitor = new SetOptionsVisitor();
            node.AcceptChildren(nocountVisitor);

            if (nocountVisitor.DetectedOptions.ContainsKey(SetOptions.NoCount.ToString()))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
