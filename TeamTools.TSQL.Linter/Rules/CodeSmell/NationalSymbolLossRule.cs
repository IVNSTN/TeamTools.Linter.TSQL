using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0791", "NATIONAL_SYMBOL_LOSS")]
    internal sealed class NationalSymbolLossRule : EvaluatorBasedRule<NationalSymbolLossViolation>
    {
        public NationalSymbolLossRule() : base()
        {
        }
    }
}
