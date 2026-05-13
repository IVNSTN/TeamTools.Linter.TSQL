using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0894", "ISOLATION_CHAOS_PER_QUERY")]
    internal sealed partial class IsolationLevelChaosPerQueryRule : AbstractRule
    {
        public IsolationLevelChaosPerQueryRule() : base()
        {
        }

        public override void Visit(StatementWithCtesAndXmlNamespaces node)
        {
           node.AcceptChildren(new IsolationLevelHintVisitor(ViolationHandlerWithMessage));
        }
    }
}
