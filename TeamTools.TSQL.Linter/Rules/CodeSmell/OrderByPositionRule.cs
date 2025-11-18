using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0295", "ORDER_BY_POSITION")]
    internal sealed class OrderByPositionRule : AbstractRule
    {
        public OrderByPositionRule() : base()
        {
        }

        public override void Visit(OrderByClause node)
        {
            int n = node.OrderByElements.Count;
            for (int i = 0; i < n; i++)
            {
                if (node.OrderByElements[i].Expression is IntegerLiteral literal)
                {
                    HandleNodeError(literal);
                }
            }
        }
    }
}
