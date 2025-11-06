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

        public override void Visit(TriggerStatementBody node)
            => node.AcceptChildren(new RaiserrorVisitor(HandleNodeError));

        private class RaiserrorVisitor : VisitorWithCallback
        {
            public RaiserrorVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(RaiseErrorStatement node) => Callback(node);
        }
    }
}
