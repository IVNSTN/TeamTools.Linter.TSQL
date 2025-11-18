using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("HD0149", "HOST_NAME_BASED_FLOW")]
    internal sealed class HostNameBasedCodeFlowRule : AbstractRule
    {
        private readonly HostNameVisitor visitor;

        public HostNameBasedCodeFlowRule() : base()
        {
            visitor = new HostNameVisitor(ViolationHandler);
        }

        public override void Visit(IfStatement node) => ValidatePredicate(node.Predicate);

        // Note SearchCondition may be null in case of WHERE CURRENT OF
        public override void Visit(WhereClause node) => ValidatePredicate(node.SearchCondition);

        public override void Visit(WhileStatement node) => ValidatePredicate(node.Predicate);

        private void ValidatePredicate(BooleanExpression node) => node?.Accept(visitor);

        private sealed class HostNameVisitor : VisitorWithCallback
        {
            private static readonly string SearchedFunc = "HOST_NAME";

            public HostNameVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(FunctionCall node)
            {
                if (node.FunctionName.Value.Equals(SearchedFunc, StringComparison.OrdinalIgnoreCase))
                {
                    Callback(node);
                }
            }
        }
    }
}
