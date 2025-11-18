using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0704", "INVALID_ARGUMENT_COUNT")]
    internal sealed class InvalidFunctionArgumentCountRule : EvaluatorBasedRule<InvalidNumberOfArgumentsViolation>
    {
        public InvalidFunctionArgumentCountRule() : base()
        {
        }
    }
}
