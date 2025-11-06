using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0101", "SELECT_INTO")]
    internal sealed class SelectIntoRule : AbstractRule
    {
        public SelectIntoRule() : base()
        {
        }

        public override void Visit(SelectStatement node) => HandleNodeErrorIfAny(node.Into);
    }
}
