using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0869", "OPTIMIZER_FIGHT_TABLE_HINT")]
    internal sealed class FightOptimizerByTableHintRule : AbstractRule
    {
        // Except locking hints.
        // Storing names to avoid unnecessary string generation by ToString()
        private static readonly Dictionary<TableHintKind, string> PerformanceHints = new Dictionary<TableHintKind, string>
        {
            { TableHintKind.ForceSeek, "FORCESEEK" },
            { TableHintKind.ForceScan, "FORCESCAN" },
            { TableHintKind.Index, "INDEX" },
        };

        public FightOptimizerByTableHintRule() : base()
        {
        }

        public override void Visit(TableHint node)
        {
            if (PerformanceHints.TryGetValue(node.HintKind, out var hint))
            {
                HandleNodeError(node, hint);
            }
        }
    }
}
