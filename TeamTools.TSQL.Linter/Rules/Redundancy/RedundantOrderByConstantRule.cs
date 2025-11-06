using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0296", "REDUNDANT_ORDER_BY_CONST")]
    internal sealed class RedundantOrderByConstantRule : AbstractRule
    {
        public RedundantOrderByConstantRule() : base()
        {
        }

        public override void Visit(OrderByClause node)
        {
            var constants = node.OrderByElements
                .Select(ord => ord.Expression)
                .Where(expr => (expr is Literal && !(expr is IntegerLiteral))
                    || (expr is VariableReference));

            if (!constants.Any())
            {
                return;
            }

            foreach (var l in constants)
            {
                HandleNodeError(l);
            }
        }
    }
}
