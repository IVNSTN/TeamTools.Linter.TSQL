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

        public override void Visit(TSqlBatch node)
        {
            node.Accept(new TopValidator(HandleNodeError));
        }

        private class TopValidator : TSqlFragmentVisitor
        {
            private readonly ICollection<TSqlFragment> ignoredQueries = new List<TSqlFragment>();
            private readonly Action<TSqlFragment, string> callback;

            public TopValidator(Action<TSqlFragment, string> callback)
            {
                this.callback = callback;
            }

            public override void Visit(ExistsPredicate node)
            {
                ignoredQueries.Add(node.Subquery.QueryExpression);
            }

            public override void Visit(QuerySpecification node)
            {
                if (ignoredQueries.Contains(node))
                {
                    return;
                }

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

                callback(node.TopRowFilter, default);
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
