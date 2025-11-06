using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
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
            var literals = node.OrderByElements
                .Select(ord => ord.Expression)
                .OfType<IntegerLiteral>();

            if (!literals.Any())
            {
                return;
            }

            foreach (var literal in literals)
            {
                HandleNodeError(literal);
            }
        }
    }
}
