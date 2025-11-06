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
        public TransactionControlInTriggerRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node)
        => node.AcceptChildren(new TranControlVisitor(HandleNodeError));

        private class TranControlVisitor : VisitorWithCallback
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
