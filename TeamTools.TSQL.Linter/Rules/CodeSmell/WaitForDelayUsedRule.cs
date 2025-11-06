using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0991", "WAITFOR_DELAY_USED")]
    internal sealed class WaitForDelayUsedRule : AbstractRule
    {
        public WaitForDelayUsedRule() : base()
        {
        }

        public override void Visit(WaitForStatement node)
        {
            if (node.Statement != null)
            {
                // not DELAY
                return;
            }

            HandleNodeError(node);
        }
    }
}
