using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0112", "SELECT_STAR")]
    internal sealed class SelectStarRule : AbstractRule
    {
        public SelectStarRule() : base()
        {
        }

        public override void Visit(SelectStarExpression node) => HandleNodeError(node);
    }
}
