using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0870", "OPTIMIZER_FIGHT_QUERY_OPTION")]
    internal sealed class FightOptimizerByQueryOptionRule : AbstractRule
    {
        public FightOptimizerByQueryOptionRule() : base()
        {
        }

        public override void Visit(OptimizerHint node)
        {
            if (node.HintKind == OptimizerHintKind.Recompile
            || node.HintKind == OptimizerHintKind.MaxRecursion)
            {
                // Allowed hints
                return;
            }

            if (node is OptimizeForOptimizerHint optimizeFor
            && optimizeFor.IsForUnknown)
            {
                // OPTIMIZE FOR UNKNOWN may fix parameter sniffig issues
                return;
            }

            if (node.HintKind == OptimizerHintKind.MaxDop
            && node is LiteralOptimizerHint maxdop
            && int.TryParse(maxdop.Value.Value, out int maxdopValue)
            && maxdopValue > 0)
            {
                // MAXDOP 0 overrides DB and Server level settings
                // otherwise it might be fine
                return;
            }

            HandleNodeError(node, node.HintKind.ToString());
        }
    }
}
