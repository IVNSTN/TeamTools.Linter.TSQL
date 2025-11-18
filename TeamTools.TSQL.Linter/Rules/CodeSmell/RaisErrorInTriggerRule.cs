using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0186", "RAISERROR_IN_TRIGGER")]
    [CompatibilityLevel(SqlVersion.Sql110)]
    [TriggerRule]
    internal sealed class RaisErrorInTriggerRule : AbstractRule
    {
        public RaisErrorInTriggerRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch batch)
        {
            // CREATE PROC/TRIGGER/FUNC must be the first statement in a batch
            var firstStmt = batch.Statements[0];
            if (firstStmt is TriggerStatementBody trg)
            {
                DoValidate(trg.StatementList);
            }
        }

        private void DoValidate(StatementList node) => node?.AcceptChildren(new RaiserrorVisitor(ViolationHandler));

        private class RaiserrorVisitor : VisitorWithCallback
        {
            public RaiserrorVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(RaiseErrorStatement node) => Callback(node);
        }
    }
}
