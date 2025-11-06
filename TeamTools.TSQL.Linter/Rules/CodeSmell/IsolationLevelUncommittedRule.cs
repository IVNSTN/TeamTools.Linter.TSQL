using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0136", "ISOLATION_UNCOMMITTED")]
    internal sealed class IsolationLevelUncommittedRule : AbstractRule
    {
        public IsolationLevelUncommittedRule() : base()
        {
        }

        public override void Visit(SetTransactionIsolationLevelStatement node)
        {
            if (node.Level != IsolationLevel.ReadUncommitted)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
