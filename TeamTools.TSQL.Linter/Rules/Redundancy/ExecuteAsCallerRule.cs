using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0811", "EXECUTE_AS_CALLER")]
    internal sealed class ExecuteAsCallerRule : AbstractRule
    {
        public ExecuteAsCallerRule() : base()
        {
        }

        public override void Visit(ExecuteAsClause node)
        {
            if (node.ExecuteAsOption == ExecuteAsOption.Caller)
            {
                HandleNodeError(node);
            }
        }
    }
}
