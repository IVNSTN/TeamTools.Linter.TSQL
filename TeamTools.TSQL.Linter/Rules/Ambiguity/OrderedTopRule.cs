using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0114", "ORDER_BY_TOP")]
    internal sealed class OrderedTopRule : AbstractRule
    {
        public OrderedTopRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            node.Accept(new TopValidator(ViolationHandler));
        }

        private sealed class TopValidator : VisitorWithCallback
        {
            private List<TSqlFragment> ignoredQueries;

            public TopValidator(Action<TSqlFragment> callback) : base(callback)
            {
            }

            public override void Visit(ExistsPredicate node)
            {
                if (ignoredQueries is null)
                {
                    ignoredQueries = new List<TSqlFragment>();
                }

                ignoredQueries.Add(node.Subquery.QueryExpression);
            }

            public override void Visit(QuerySpecification node)
            {
                if (node.TopRowFilter is null)
                {
                    // no TOP
                    return;
                }

                if (node.OrderByClause != null && node.OrderByClause.OrderByElements.Count > 0)
                {
                    // TOP from sorted resultset
                    return;
                }

                if (node.UniqueRowFilter.IsDistinct())
                {
                    // DISTINCT sorts
                    return;
                }

                if (ignoredQueries != null && ignoredQueries.Contains(node))
                {
                    return;
                }

                if (GetTopExpression(node.TopRowFilter.Expression) is IntegerLiteral intl
                && int.TryParse(intl.Value, out int topLimit) && topLimit == 0)
                {
                    // ignoring TOP 0 - no rows don't need to be sorted
                    return;
                }

                if ((node.SelectElements.Count == 1) && (node.SelectElements[0] is SelectScalarExpression expr))
                {
                    // a scalar thing needs no order
                    if (expr.Expression is Literal || expr.Expression is VariableReference)
                    {
                        return;
                    }
                }

                Callback(node.TopRowFilter);
            }

            private static ScalarExpression GetTopExpression(ScalarExpression node)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is UnaryExpression ue)
                {
                    return GetTopExpression(ue.Expression);
                }

                return node;
            }
        }
    }
}
