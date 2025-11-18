using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0514", "SHOWING_STATS")]
    internal sealed class ShowStatisticsFromCodeRule : AbstractRule
    {
        private readonly StatsVisitor statsVisitor;

        public ShowStatisticsFromCodeRule() : base()
        {
            statsVisitor = new StatsVisitor(ViolationHandler);
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is ProcedureStatementBody proc)
            {
                DetectStatsUsage(proc.StatementList);
            }
            else if (firstStmt is TriggerStatementBody trg)
            {
                DetectStatsUsage(trg.StatementList);
            }
        }

        private void DetectStatsUsage(TSqlFragment node) => node?.AcceptChildren(statsVisitor);

        private sealed class StatsVisitor : VisitorWithCallback
        {
            public StatsVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(SetStatisticsStatement node) => Callback(node);
        }
    }
}
