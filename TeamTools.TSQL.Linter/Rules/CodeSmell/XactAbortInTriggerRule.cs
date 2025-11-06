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
        public XactAbortInTriggerRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node)
        {
            if (node.StatementList?.Statements?.Count == 0)
            {
                return;
            }

            var optDetector = new SetXactVisitor(HandleNodeError);
            node.StatementList.Accept(optDetector);
        }

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
