using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0860", "HISTORY_PERIOD_SET")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class TemporalTablePeriodDefinedRule : AbstractRule
    {
        public TemporalTablePeriodDefinedRule()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.Definition is null)
            {
                // e.g. filetable
                return;
            }

            if (node.Definition.SystemTimePeriod != null)
            {
                // PERIOD defined
                return;
            }

            for (int i = node.Options.Count - 1; i >= 0; i--)
            {
                // If no such option then this is not a temporal table
                if (node.Options[i] is SystemVersioningTableOption history)
                {
                    HandleNodeError(history);
                }
            }
        }
    }
}
