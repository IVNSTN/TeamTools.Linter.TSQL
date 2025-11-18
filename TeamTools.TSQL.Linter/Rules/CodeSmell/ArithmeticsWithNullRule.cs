using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0710", "NULL_ARITHMETICS")]
    internal sealed class ArithmeticsWithNullRule : EvaluatorBasedRule<NullArithmeticsViolation>
    {
        public ArithmeticsWithNullRule() : base()
        {
        }
    }
}
