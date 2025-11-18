using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CV0237", "UNION_SCALAR_TO_VALUES")]
    internal sealed class UnionScalarsToValuesRule : AbstractRule
    {
        public UnionScalarsToValuesRule() : base()
        {
        }

        protected override void ValidateBatch(TSqlBatch node)
            => node.Accept(new CollapsibleSelectDetector(ViolationHandler));

        private class CollapsibleSelectDetector : TSqlFragmentVisitor
        {
            private readonly List<TSqlFragment> analyzedNodes = new List<TSqlFragment>();

            public CollapsibleSelectDetector(Action<TSqlFragment> callback)
            {
                Callback = callback;
            }

            private Action<TSqlFragment> Callback { get; }

            public override void Visit(BinaryQueryExpression node)
            {
                if (analyzedNodes.Any(nd => nd.FirstTokenIndex <= node.FirstTokenIndex && nd.LastTokenIndex >= node.LastTokenIndex))
                {
                    // already reported on this element
                    return;
                }

                if (IsConvertibleToValues(node))
                {
                    Callback(node);
                    analyzedNodes.Add(node);
                }
                else if (node.FirstQueryExpression is BinaryQueryExpression bin
                && IsConvertibleToValues(bin.SecondQueryExpression)
                && IsConvertibleToValues(node.SecondQueryExpression))
                {
                    // BinaryQueryExpression structure is <all but last union parts> + the last query
                    // then the nested BinaryQueryExpression for the first expression is the same
                    // <all but last union parts> and the last query as SecondExpression.
                    // To catch collapsible unions somewhere in the end or in the middle of a long
                    // union we have to do somethind like defined in the if predicate above.
                    Callback(node.SecondQueryExpression);
                    analyzedNodes.Add(node);
                }
            }

            private static bool IsValuesSource(TSqlFragment node)
                => node is InlineDerivedTable der && der.RowValues != null;

            // TODO : get rid of recursion
            private static bool IsConvertibleToValues(QueryExpression node)
            {
                if (node is BinaryQueryExpression bin)
                {
                    return bin.BinaryQueryExpressionType == BinaryQueryExpressionType.Union
                        && IsConvertibleToValues(bin.FirstQueryExpression)
                        && IsConvertibleToValues(bin.SecondQueryExpression);
                }

                if (node is QuerySpecification spec)
                {
                    // not trying to guess how to implement the same on VALUES
                    if (spec.WhereClause != null || spec.ForClause != null)
                    {
                        return false;
                    }

                    if (spec.FromClause != null
                    && (spec.FromClause.TableReferences.Count != 1
                    || !IsValuesSource(spec.FromClause.TableReferences[0])))
                    {
                        return false;
                    }

                    // select * from values
                    // or list of scalar values without from
                    return !spec.SelectElements
                        .Any(el => !(
                        (el is SelectStarExpression && spec.FromClause?.TableReferences.Count == 1)
                        || (el is SelectScalarExpression expr && IsScalarVarOrLiteral(expr.Expression))));
                }

                return false;
            }

            private static bool IsScalarVarOrLiteral(ScalarExpression node)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is BinaryExpression bin)
                {
                    return IsScalarVarOrLiteral(bin.FirstExpression)
                        && IsScalarVarOrLiteral(bin.SecondExpression);
                }

                if (node is ScalarSubquery q)
                {
                    // TODO : not sure
                    return IsConvertibleToValues(q.QueryExpression);
                }

                if (node is VariableReference)
                {
                    // keep these true-blocks - easier to debug
                    return true;
                }

                if (node is Literal)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
