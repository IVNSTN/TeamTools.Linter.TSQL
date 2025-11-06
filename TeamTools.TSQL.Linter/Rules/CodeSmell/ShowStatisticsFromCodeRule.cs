using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0514", "SHOWING_STATS")]
    internal sealed class ShowStatisticsFromCodeRule : AbstractRule
    {
        public ShowStatisticsFromCodeRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node) => DetectStatsUsage(node);

        public override void Visit(ProcedureStatementBody node) => DetectStatsUsage(node);

        private void DetectStatsUsage(TSqlFragment node)
        => node.AcceptChildren(new StatsVisitor(HandleNodeError));

        private class StatsVisitor : VisitorWithCallback
        {
            public StatsVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(SetStatisticsStatement node) => Callback(node);
        }
    }
}
