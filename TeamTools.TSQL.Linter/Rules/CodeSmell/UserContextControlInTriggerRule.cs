using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0188", "USER_CONTEXT_CONTROL_IN_TRIGGER")]
    [TriggerRule]
    [SecurityRule]
    internal sealed class UserContextControlInTriggerRule : AbstractRule
    {
        public UserContextControlInTriggerRule() : base()
        {
        }

        public override void Visit(TriggerStatementBody node)
            => node.AcceptChildren(new SetUserContextDetector(HandleNodeError));

        public override void Visit(ExecuteAsTriggerOption node)
        {
            if (node.ExecuteAsClause.ExecuteAsOption == ExecuteAsOption.Caller)
            {
                return;
            }

            HandleNodeError(node);
        }

        private class SetUserContextDetector : VisitorWithCallback
        {
            public SetUserContextDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(ExecuteAsClause node) => Callback(node);

            public override void Visit(SetUserStatement node) => Callback(node);

            public override void Visit(RevertStatement node) => Callback(node);
        }
    }
}
