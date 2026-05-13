using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0868", "OPTIMIZER_FIGHT_JOIN_HINT")]
    internal sealed class FightOptimizerByJoinHintRule : AbstractRule
    {
        // Except None and Remote
        // Storing names to avoid unnecessary string generation by ToString()
        private static readonly Dictionary<JoinHint, string> JoinHints = new Dictionary<JoinHint, string>
        {
            { JoinHint.Loop, "LOOP" },
            { JoinHint.Hash, "HASH" },
            { JoinHint.Merge, "MERGE" },
        };

        public FightOptimizerByJoinHintRule() : base()
        {
        }

        public override void Visit(QualifiedJoin node)
        {
            if (JoinHints.TryGetValue(node.JoinHint, out string hint))
            {
                HandleNodeError(node, hint);
            }
        }
    }
}
