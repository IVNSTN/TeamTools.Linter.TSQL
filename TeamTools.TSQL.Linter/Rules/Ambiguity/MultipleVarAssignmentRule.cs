using TeamTools.Common.Linting;
using TeamTools.TSQL.ExpressionEvaluator.Violations;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0996", "MULTI_SET_SAME_VAR")]
    internal sealed class MultipleVarAssignmentRule : EvaluatorBasedRule<AmbiguousVariableMultipleAssignment>
    {
        public MultipleVarAssignmentRule() : base()
        {
        }
    }
}
