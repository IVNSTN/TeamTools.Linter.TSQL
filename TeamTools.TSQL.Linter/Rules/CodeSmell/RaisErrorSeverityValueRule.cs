using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0176", "RAISERROR_VALID_SEVERITY")]
    internal sealed class RaisErrorSeverityValueRule : AbstractRule
    {
        private const int MinCorrectSeverityValue = 0;
        private const int MaxCorrectSeverityValue = 17;

        public RaisErrorSeverityValueRule() : base()
        {
        }

        public override void Visit(RaiseErrorStatement node) => ValidateErrorSeverity(node.SecondParameter);

        // TODO : use ExpressionEvaluator
        private static int? ExtractValue(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is UnaryExpression un)
            {
                return ExtractValue(un.Expression)
                    * (un.UnaryExpressionType == UnaryExpressionType.Negative ? -1 : +1);
            }

            if (expr is IntegerLiteral i && int.TryParse(i.Value, out int res))
            {
                return res;
            }

            return default;
        }

        private void ValidateErrorSeverity(ScalarExpression node)
        {
            var severityValue = ExtractValue(node);
            if (severityValue is null)
            {
                return;
            }

            if (severityValue < MinCorrectSeverityValue || severityValue > MaxCorrectSeverityValue)
            {
                HandleNodeError(node, severityValue.ToString());
            }
        }
    }
}
