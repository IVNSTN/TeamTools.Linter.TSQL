using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0989", "SCALAR_QUERY_MULTIPLE_COL")]
    internal sealed class MultipleColumnsInScalarSubqueryRule : AbstractRule
    {
        public MultipleColumnsInScalarSubqueryRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        => node.Accept(new BrokenScalarQueryVisitor(ViolationHandler));

        private sealed class BrokenScalarQueryVisitor : VisitorWithCallback
        {
            private List<TSqlFragment> ignoredElements;

            public BrokenScalarQueryVisitor(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(ExistsPredicate node) => (ignoredElements ?? (ignoredElements = new List<TSqlFragment>())).Add(node.Subquery);

            public override void Visit(ScalarSubquery node)
            {
                if (ignoredElements != null && ignoredElements.Contains(node))
                {
                    // EXISTS is fine with multiple columns
                    return;
                }

                if (node.QueryExpression.ForClause != null)
                {
                    // FOR XML, FOR JSON will return scalar value
                    return;
                }

                var q = node.QueryExpression.GetQuerySpecification();
                if (q != null && q.SelectElements.Count > 1)
                {
                    Callback(q.SelectElements[1]);
                }
            }
        }
    }
}
