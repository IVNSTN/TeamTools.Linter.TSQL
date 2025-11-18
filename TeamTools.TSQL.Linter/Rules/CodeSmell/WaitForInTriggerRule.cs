using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0189", "WAITFOR_IN_TRIGGER")]
    [TriggerRule]
    internal sealed class WaitForInTriggerRule : AbstractRule
    {
        private readonly WaitForVisitor waitForDetector;

        public WaitForInTriggerRule() : base()
        {
            waitForDetector = new WaitForVisitor(ViolationHandler);
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

        private void DoValidate(StatementList node) => node?.AcceptChildren(waitForDetector);

        private class WaitForVisitor : VisitorWithCallback
        {
            public WaitForVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(WaitForStatement node) => Callback(node);
        }
    }
}
