using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0707", "REDUNDANT_ARGUMENT")]
    internal sealed class RedundantFunctionArgumentRule : EvaluatorBasedRule<RedundantFunctionArgumentViolation>
    {
        public RedundantFunctionArgumentRule() : base()
        {
        }
    }
}
