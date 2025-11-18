using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0151", "COMMIT_IN_CATCH")]
    internal sealed class CommitInCatchRule : AbstractRule
    {
        private readonly CommitVisitor visitor;

        public CommitInCatchRule() : base()
        {
            visitor = new CommitVisitor(ViolationHandler);
        }

        public override void Visit(TryCatchStatement node) => node.CatchStatements.Accept(visitor);

        private class CommitVisitor : VisitorWithCallback
        {
            public CommitVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(CommitTransactionStatement node) => Callback(node);
        }
    }
}
