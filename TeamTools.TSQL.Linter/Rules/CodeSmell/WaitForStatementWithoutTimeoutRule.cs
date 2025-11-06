using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0992", "WAITFOR_STMT_TIMEOUT")]
    internal sealed class WaitForStatementWithoutTimeoutRule : AbstractRule
    {
        public WaitForStatementWithoutTimeoutRule() : base()
        {
        }

        public override void Visit(WaitForStatement node)
        {
            if (node.Statement is null)
            {
                // DELAY
                return;
            }

            if (node.Timeout != null)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
