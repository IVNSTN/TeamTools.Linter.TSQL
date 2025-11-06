using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0953", "TOP_TAKES_ALL")]
    internal sealed class TopTakesAllRule : AbstractRule
    {
        private const int MaxPercentValue = 90;
        private const int MaxAbsoluteValue = 10000;

        public TopTakesAllRule() : base()
        {
        }

        public override void Visit(TopRowFilter node)
        {
            var topValueExpr = GetValueExpression(node.Expression);
            if (topValueExpr is null
            || !(topValueExpr is IntegerLiteral topValueText)
            || !int.TryParse(topValueText.Value, out int topValue))
            {
                return;
            }

            if ((node.Percent && topValue <= MaxPercentValue)
            || (!node.Percent && topValue <= MaxAbsoluteValue))
            {
                return;
            }

            string suffix = node.Percent ? "%" : "";

            HandleNodeError(node, $"{topValue}{suffix}");
        }

        private static ValueExpression GetValueExpression(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            if (node is ValueExpression ve)
            {
                return ve;
            }

            return default;
        }
    }
}
