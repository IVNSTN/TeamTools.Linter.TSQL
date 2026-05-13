using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0861", "HISTORY_SET_STORAGE_NAME")]
    [CompatibilityLevel(SqlVersion.Sql130)]
    internal sealed class TemporalTableNameHistoryTableRule : AbstractRule
    {
        public TemporalTableNameHistoryTableRule()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            for (int i = node.Options.Count - 1; i >= 0; i--)
            {
                if (node.Options[i] is SystemVersioningTableOption history
                && history.HistoryTable is null)
                {
                    HandleNodeError(history);
                }
            }
        }
    }
}
