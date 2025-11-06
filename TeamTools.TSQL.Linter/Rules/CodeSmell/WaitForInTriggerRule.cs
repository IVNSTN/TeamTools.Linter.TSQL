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
        public WaitForInTriggerRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node)
            => node.AcceptChildren(new WaitForVisitor(HandleNodeError));

        private class WaitForVisitor : VisitorWithCallback
        {
            public WaitForVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(WaitForStatement node) => Callback(node);
        }
    }
}
