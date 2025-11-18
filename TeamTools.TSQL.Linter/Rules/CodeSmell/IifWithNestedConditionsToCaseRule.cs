using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0299", "COMPLICATED_IIF_TO_CASE")]
    [CompatibilityLevel(SqlVersion.Sql110)]
    internal sealed class IifWithNestedConditionsToCaseRule : AbstractRule
    {
        private readonly NestedConditionsVisitor visitor;

        public IifWithNestedConditionsToCaseRule() : base()
        {
            visitor = new NestedConditionsVisitor(ViolationHandler);
        }

        public override void Visit(IIfCall node) => node.AcceptChildren(visitor);

        private sealed class NestedConditionsVisitor : VisitorWithCallback
        {
            public NestedConditionsVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(QuerySpecification node) => Callback(node);

            public override void Visit(IIfCall node) => Callback(node);

            public override void Visit(CaseExpression node) => Callback(node);
        }
    }
}
