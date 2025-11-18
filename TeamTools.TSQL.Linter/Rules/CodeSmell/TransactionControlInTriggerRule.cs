using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0187", "TRAN_CONTROL_IN_TRIGGER")]
    [TriggerRule]
    internal sealed class TransactionControlInTriggerRule : AbstractRule
    {
        private readonly TranControlVisitor tranDetector;

        public TransactionControlInTriggerRule() : base()
        {
            tranDetector = new TranControlVisitor(ViolationHandler);
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

        private void DoValidate(StatementList node) => node?.AcceptChildren(tranDetector);

        private sealed class TranControlVisitor : VisitorWithCallback
        {
            public TranControlVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(CommitTransactionStatement node) => Callback(node);

            public override void Visit(RollbackTransactionStatement node) => Callback(node);

            public override void Visit(BeginTransactionStatement node) => Callback(node);

            public override void Visit(SaveTransactionStatement node) => Callback(node);
        }
    }
}
