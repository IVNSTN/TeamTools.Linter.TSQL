using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0871", "OPTIMIZER_FIGHT_FORCE_PLAN")]
    internal sealed class FightOptimizerByForcePlanRule : AbstractRule
    {
        public FightOptimizerByForcePlanRule() : base()
        {
        }

        public override void Visit(PredicateSetStatement node)
        {
            if (node.Options.HasFlag(SetOptions.ForcePlan))
            {
                HandleNodeError(node);
            }
        }

        public override void Visit(OptimizerHint node)
        {
            if (node.HintKind == OptimizerHintKind.UsePlan)
            {
                HandleNodeError(node);
            }
        }
    }
}
