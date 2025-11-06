using Microsoft.SqlServer.TransactSql.ScriptDom;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DM0513", "STATISTICS_MANAGEMENT")]
    internal sealed class StatisticsManagementRule : AbstractRule
    {
        public StatisticsManagementRule() : base()
        {
        }

        public override void Visit(CreateStatisticsStatement node) => HandleNodeError(node);

        public override void Visit(DropStatisticsStatement node) => HandleNodeError(node);
    }
}
