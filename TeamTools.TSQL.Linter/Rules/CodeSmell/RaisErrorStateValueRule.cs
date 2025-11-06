using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0177", "RAISERROR_VALID_STATE")]
    internal class RaisErrorStateValueRule : AbstractRule
    {
        private const int MinCorrectStateValue = 0;
        private const int MaxCorrectStateValue = 255;

        public RaisErrorStateValueRule() : base()
        {
        }

        public override void Visit(RaiseErrorStatement node) => ValidateErrorState(node.ThirdParameter);

        public override void Visit(ThrowStatement node) => ValidateErrorState(node.State);

        // TODO : use ExpressionEvaluator
        private static int? ExtractValue(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is UnaryExpression un)
            {
                if (un.UnaryExpressionType == UnaryExpressionType.Negative)
                {
                    return -ExtractValue(un.Expression);
                }
                else
                {
                    return ExtractValue(un.Expression);
                }
            }

            if (expr is IntegerLiteral i && int.TryParse(i.Value, out int res))
            {
                return res;
            }

            return default;
        }

        private void ValidateErrorState(ScalarExpression node)
        {
            var stateValue = ExtractValue(node);
            if (stateValue is null)
            {
                return;
            }

            if (stateValue < MinCorrectStateValue || stateValue > MaxCorrectStateValue)
            {
                HandleNodeError(node, stateValue.ToString());
            }
        }
    }
}
