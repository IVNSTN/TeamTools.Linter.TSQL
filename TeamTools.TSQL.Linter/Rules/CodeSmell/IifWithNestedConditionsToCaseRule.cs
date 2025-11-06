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
        public IifWithNestedConditionsToCaseRule() : base()
        {
        }

        public override void Visit(IIfCall node)
            => node.AcceptChildren(new NestedConditionsVisitor(HandleNodeError));

        private class NestedConditionsVisitor : VisitorWithCallback
        {
            public NestedConditionsVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(QuerySpecification node) => Callback(node);

            public override void Visit(IIfCall node) => Callback(node);

            public override void Visit(CaseExpression node) => Callback(node);
        }
    }
}
