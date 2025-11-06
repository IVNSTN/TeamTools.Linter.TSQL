using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0288", "REDUNDANT_SELECT_SCALAR")]
    internal sealed class RedundantSelectScalarRule : AbstractRule
    {
        public RedundantSelectScalarRule() : base()
        {
        }

        public override void Visit(TSqlBatch node)
        {
            var redundantSelectCatcher = new RedundantSelectCatcher();
            node.AcceptChildren(redundantSelectCatcher);
            var scalarVisitor = new SelectScalarVisitor(redundantSelectCatcher.IgnoredExpressions, HandleNodeError);
            node.AcceptChildren(scalarVisitor);
        }

        // TODO : ignore INSERT ... SELECT @scalar ?
        private class RedundantSelectCatcher : TSqlFragmentVisitor
        {
            private readonly List<ScalarExpression> ignoredExpressions = new List<ScalarExpression>();

            public List<ScalarExpression> IgnoredExpressions => ignoredExpressions;

            public override void Visit(OrderByClause node)
            {
                IgnoredExpressions.AddRange(node.OrderByElements
                    .Select(sort => ExtractSelectScalarExpressionIfAny(sort.Expression, true))
                    .Where(expr => expr != null));
            }

            public override void Visit(OutputClause node)
            {
                // output is after data modification and reflects made changes even if cols are ignored
                DoIgnoreSelectedElements(node.SelectColumns);
            }

            public override void Visit(OutputIntoClause node)
            {
                // output is after data modification and reflects made changes even if cols are ignored
                DoIgnoreSelectedElements(node.SelectColumns);
            }

            public override void Visit(StatementWithCtesAndXmlNamespaces node)
            {
                if ((node.WithCtesAndXmlNamespaces?.CommonTableExpressions?.Count ?? 0) == 0)
                {
                    return;
                }

                // ctes may be used in join, unions, recursion thus ignoring them
                foreach (var cte in node.WithCtesAndXmlNamespaces.CommonTableExpressions)
                {
                    var spec = cte.QueryExpression.GetQuerySpecification();
                    if (spec is null)
                    {
                        continue;
                    }

                    // nested select-scalar may be illegal so no deep digging
                    DoIgnoreSelectedElements(spec.SelectElements);
                }
            }

            public override void Visit(SelectStatement node)
            {
                if (node.Into != null)
                {
                    return;
                }

                if (!(node.QueryExpression is QuerySpecification spec))
                {
                    return;
                }

                // ignoring top-level selects (resultsets)
                DoIgnoreSelectedElements(spec.SelectElements);
            }

            public override void Visit(QuerySpecification node)
            {
                // ignoring only "normal" selects
                if (node.FromClause is null && node.WhereClause is null && node.ForClause is null
                && node.SelectElements.Count == 1)
                {
                    return;
                }

                DoIgnoreSelectedElements(node.SelectElements);
            }

            public override void Visit(BinaryQueryExpression node)
            {
                // UNION SELECT <SCALAR> cannot be rewritten directly without SELECT
                IgnoreSelectScalarFromUnion(node);
            }

            public override void Visit(UnqualifiedJoin node)
            {
                // applies improve DRY sometimes
                if (!(node.SecondTableReference is QueryDerivedTable q
                && q.QueryExpression is QuerySpecification spec))
                {
                    return;
                }

                DoIgnoreSelectedElements(spec.SelectElements);
            }

            private void DoIgnoreSelectedElements(IList<SelectElement> select)
            {
                IgnoredExpressions.AddRange(select
                    .OfType<SelectScalarExpression>()
                    .Select(el => el.Expression));
            }

            private void IgnoreSelectScalarFromUnion(QueryExpression node)
            {
                if (node is BinaryQueryExpression bin)
                {
                    IgnoreSelectScalarFromUnion(bin.FirstQueryExpression);
                    IgnoreSelectScalarFromUnion(bin.SecondQueryExpression);
                }
                else if (node is QuerySpecification spec)
                {
                    IgnoredExpressions.AddRange(spec.SelectElements
                        .OfType<SelectScalarExpression>()
                        .Select(el => el.Expression));
                }
            }

            private ScalarExpression ExtractSelectScalarExpressionIfAny(TSqlFragment node, bool diveIntoSubQueries = true)
            {
                if (node is ParenthesisExpression p)
                {
                    return ExtractSelectScalarExpressionIfAny(p.Expression);
                }
                else if (diveIntoSubQueries
                    && node is ScalarSubquery q
                    && q.QueryExpression is QuerySpecification qs
                    && qs.SelectElements.Count == 1)
                {
                    return ExtractSelectScalarExpressionIfAny(qs.SelectElements[0]);
                }
                else if (node is SelectScalarExpression sse)
                {
                    return sse.Expression;
                }
                else
                {
                    return null;
                }
            }
        }

        private class SelectScalarVisitor : TSqlFragmentVisitor
        {
            private static readonly int MaxReadableLength = 32;
            private static readonly List<TSqlTokenType> ScriptDelimiters;
            private readonly IList<ScalarExpression> ignoredExpressions;
            private readonly Action<TSqlFragment, string> callback;

            static SelectScalarVisitor()
            {
                ScriptDelimiters = new List<TSqlTokenType>
                {
                    TSqlTokenType.LeftParenthesis,
                    TSqlTokenType.Comma,
                };
            }

            public SelectScalarVisitor(IList<ScalarExpression> ignoredExpressions, Action<TSqlFragment, string> callback)
            {
                this.ignoredExpressions = ignoredExpressions;
                this.callback = callback;
            }

            public override void Visit(SelectScalarExpression node)
            {
                if (ignoredExpressions.Contains(node.Expression))
                {
                    return;
                }

                if (node.ColumnName != null)
                {
                    return;
                }

                if (IsColumnReferencingExpression(node.Expression))
                {
                    return;
                }

                if (node.Expression is FunctionCall fn && fn.CallTarget != null
                    && fn.CallTarget is ExpressionCallTarget expr && expr.Expression is ScalarSubquery)
                {
                    return;
                }

                callback(node, GetExpressionText(node.Expression));
            }

            private static string GetExpressionText(ScalarExpression node)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is BinaryExpression bin)
                {
                    // part of expression is good enough
                    return GetExpressionText(bin.FirstExpression);
                }

                if (node is VariableReference vr)
                {
                    return vr.Name;
                }

                if (node is FunctionCall fn)
                {
                    // only called function name is good enough
                    return fn.CallTarget != null
                        ? fn.CallTarget.GetFragmentText() + "." + fn.FunctionName.Value
                        : fn.FunctionName.Value;
                }

                return node.GetFragmentText(ScriptDelimiters).Left(MaxReadableLength);
            }

            private static bool IsColumnReferencingExpression(ScalarExpression expr)
            {
                while (expr is ParenthesisExpression pe)
                {
                    expr = pe.Expression;
                }

                if (expr is BinaryExpression be)
                {
                    return IsColumnReferencingExpression(be.FirstExpression)
                        || IsColumnReferencingExpression(be.SecondExpression);
                }

                return expr is ColumnReferenceExpression;
            }
        }
    }
}
