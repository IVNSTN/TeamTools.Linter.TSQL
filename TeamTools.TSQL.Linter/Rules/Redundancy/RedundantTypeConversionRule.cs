using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0706", "REDUNDANT_TYPE_CONVERSION")]
    internal sealed class RedundantTypeConversionRule : EvaluatorBasedRule<RedundantTypeConversionViolation>
    {
        public RedundantTypeConversionRule() : base()
        {
        }
    }
}
