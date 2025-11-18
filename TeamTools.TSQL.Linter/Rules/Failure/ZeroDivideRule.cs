using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0708", "ZERO_DIVIDE")]
    internal sealed class ZeroDivideRule : EvaluatorBasedRule<DivideByZeroViolation>
    {
        public ZeroDivideRule() : base()
        {
        }
    }
}
