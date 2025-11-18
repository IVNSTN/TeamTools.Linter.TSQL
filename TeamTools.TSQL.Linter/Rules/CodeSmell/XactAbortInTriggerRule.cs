using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0737", "XACT_ABORT_IN_TRIGGER")]
    [TriggerRule]
    internal sealed class XactAbortInTriggerRule : AbstractRule
    {
        private readonly SetXactVisitor optDetector;

        public XactAbortInTriggerRule() : base()
        {
            optDetector = new SetXactVisitor(ViolationHandler);
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

        private void DoValidate(StatementList node) => node?.AcceptChildren(optDetector);

        private class SetXactVisitor : VisitorWithCallback
        {
            public SetXactVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(PredicateSetStatement node)
            {
                if (node.Options.HasFlag(SetOptions.XactAbort))
                {
                    Callback(node);
                }
            }
        }
    }
}
