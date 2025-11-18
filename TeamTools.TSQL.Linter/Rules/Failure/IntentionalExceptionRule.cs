using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0709", "INTENTIONAL_EXCEPTION")]
    internal sealed class IntentionalExceptionRule : EvaluatorBasedRule<IntentionalExceptionViolation>
    {
        public IntentionalExceptionRule() : base()
        {
        }
    }
}
