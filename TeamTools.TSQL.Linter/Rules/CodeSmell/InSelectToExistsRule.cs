using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0938", "EXISTS_INSTEAD_OF_IN")]
    internal sealed class InSelectToExistsRule : AbstractRule
    {
        public InSelectToExistsRule() : base()
        {
        }

        public override void Visit(InPredicate node) => HandleNodeErrorIfAny(node.Subquery);
    }
}
