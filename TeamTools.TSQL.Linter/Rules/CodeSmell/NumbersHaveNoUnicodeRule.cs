using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0794", "NUMBER_HAS_NO_UNICODE")]
    internal sealed class NumbersHaveNoUnicodeRule : EvaluatorBasedRule<NumbersHaveNoUnicodeViolation>
    {
        public NumbersHaveNoUnicodeRule() : base()
        {
        }
    }
}
