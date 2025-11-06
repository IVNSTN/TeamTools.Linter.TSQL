using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("MA0115", "ORDER_BY_CURSOR")]
    [CursorRule]
    internal sealed class OrderedCursorRule : AbstractRule
    {
        public OrderedCursorRule() : base()
        {
        }

        public override void Visit(CursorDefinition node)
        {
            if (node.Select.QueryExpression.OrderByClause != null)
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
