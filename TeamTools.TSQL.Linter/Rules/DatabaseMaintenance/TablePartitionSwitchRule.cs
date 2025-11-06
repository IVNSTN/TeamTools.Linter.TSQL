using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DM0517", "TABLE_PARTITIONING")]
    internal sealed class TablePartitionSwitchRule : AbstractRule
    {
        public TablePartitionSwitchRule() : base()
        {
        }

        public override void Visit(AlterTableSwitchStatement node)
        {
            HandleNodeError(node);
        }
    }
}
