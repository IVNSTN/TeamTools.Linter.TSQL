using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0127", "UNKNOWN_IDENTIFIER")]
    internal sealed class UnknownIdentifierRule : AbstractRule
    {
        public UnknownIdentifierRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
        {
            var visitor = new UknownIdentifierCatcher(ViolationHandler);
            node.AcceptChildren(visitor);
        }

        private sealed class UknownIdentifierCatcher : VisitorWithCallback
        {
            public UknownIdentifierCatcher(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void ExplicitVisit(QuerySpecification node) { }

            // BinaryQueryExpression (UNION/EXCEPT etc) stores OrderBy outside of QuerySpecifications
            public override void ExplicitVisit(OrderByClause node) { }

            public override void ExplicitVisit(CreateIndexStatement node) { }

            public override void ExplicitVisit(IndexDefinition node) { }

            public override void ExplicitVisit(TableDefinition node) { }

            public override void ExplicitVisit(InsertSpecification node) { }

            public override void ExplicitVisit(UpdateSpecification node) { }

            public override void ExplicitVisit(DeleteSpecification node) { }

            public override void ExplicitVisit(MergeSpecification node) { }

            public override void ExplicitVisit(ReceiveStatement node) { }

            public override void ExplicitVisit(CreateStatisticsStatement node) { }

            // unknown things outside of expressions where column reference is possible
            // are parsed as ColumnReferenceExpression - all of them are illegal
            public override void Visit(ColumnReferenceExpression node) => Callback(node);
        }
    }
}
