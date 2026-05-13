using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0862", "HISTORY_CONSISTENCY_CHECK")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class TemporalTableConsistencyCheckRule : AbstractRule
    {
        public TemporalTableConsistencyCheckRule()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            for (int i = node.Options.Count - 1; i >= 0; i--)
            {
                if (node.Options[i] is SystemVersioningTableOption history
                && history.ConsistencyCheckEnabled == OptionState.Off)
                {
                    HandleNodeError(history);
                    return;
                }
            }
        }
    }
}
