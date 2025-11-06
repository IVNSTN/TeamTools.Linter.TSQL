using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0757", "DEADLOCK_PRIORITY_OUT_OF_RANGE")]
    internal sealed class DeadlockPriorityValueRangeRule : AbstractRule
    {
        private static readonly int MinDeadlockValue = -10;
        private static readonly int MaxDeadlockValue = 10;

        public DeadlockPriorityValueRangeRule() : base()
        {
        }

        // TODO : Utilize ExpressionEvaluator if parameter is variable
        public override void Visit(GeneralSetCommand node)
        {
            if (node.CommandType != GeneralSetCommandType.DeadlockPriority)
            {
                return;
            }

            var deadlockPriority = ExtractValue(node.Parameter);

            if (deadlockPriority != null && (deadlockPriority < MinDeadlockValue || deadlockPriority > MaxDeadlockValue))
            {
                HandleNodeError(node, deadlockPriority.ToString());
            }
        }

        private static int? ExtractValue(ScalarExpression expr)
        {
            while (expr is ParenthesisExpression pe)
            {
                expr = pe.Expression;
            }

            if (expr is UnaryExpression un)
            {
                return ExtractValue(un.Expression)
                    * (un.UnaryExpressionType == UnaryExpressionType.Negative ? -1 : 1);
            }

            if (expr is IntegerLiteral i && int.TryParse(i.Value, out int result))
            {
                return result;
            }

            return null;
        }
    }
}
