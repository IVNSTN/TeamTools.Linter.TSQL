using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal sealed partial class UnionScalarsToValuesRule
    {
        private sealed class CollapsibleSelectDetector : VisitorWithCallback
        {
            private int lastReportedTokenIndex = -1;

            public CollapsibleSelectDetector(Action<TSqlFragment> callback) : base(callback)
            { }

            public override void Visit(BinaryQueryExpression node)
            {
                if (node.FirstTokenIndex < lastReportedTokenIndex)
                {
                    // already reported on this element: the recursion might have met
                    // some nodes before visitor reaches them
                    return;
                }

                if (node.BinaryQueryExpressionType != BinaryQueryExpressionType.Union)
                {
                    // EXCEPT, INTERSECT are not our case
                    return;
                }

                if (IsConvertibleToValues(node, node.All))
                {
                    ReportNode(node);
                }
                else if (node.FirstQueryExpression is BinaryQueryExpression bin
                && IsConvertibleToValues(bin.SecondQueryExpression, bin.All)
                && IsConvertibleToValues(node.SecondQueryExpression, bin.All))
                {
                    // BinaryQueryExpression structure is <all but last union parts> + the last query
                    // then the nested BinaryQueryExpression for the first expression is the same
                    // <all but last union parts> and the last query as SecondExpression.
                    // To catch collapsible unions somewhere in the end or in the middle of a long
                    // union we have to do somethind like defined in the if predicate above.
                    ReportNode(node.SecondQueryExpression);
                }
            }

            private static bool IsValuesSource(TSqlFragment node)
                => node is InlineDerivedTable der && der.RowValues != null;

            private static bool IsConvertibleToValues(QueryExpression node, bool isUnionAll)
            {
                if (node is BinaryQueryExpression bin)
                {
                    return bin.BinaryQueryExpressionType == BinaryQueryExpressionType.Union
                        && IsConvertibleToValues(bin.FirstQueryExpression, bin.All && isUnionAll)
                        && IsConvertibleToValues(bin.SecondQueryExpression, bin.All && isUnionAll);
                }

                if (node is QuerySpecification spec)
                {
                    // not trying to guess how to implement the same on VALUES
                    if (spec.WhereClause != null || spec.ForClause != null)
                    {
                        return false;
                    }

                    if (spec.FromClause != null
                    && !(spec.FromClause.TableReferences.Count == 1
                        && IsValuesSource(spec.FromClause.TableReferences[0])))
                    {
                        return false;
                    }

                    // select * from values
                    // or list of scalar values without from
                    return AllSelectedElementsAreScalarVarsOrLiterals(spec.SelectElements, isUnionAll);
                }

                return false;
            }

            private static bool IsScalarVarOrLiteral(ScalarExpression node, bool isUnionAll)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is BinaryExpression bin)
                {
                    return IsScalarVarOrLiteral(bin.FirstExpression, isUnionAll)
                        && IsScalarVarOrLiteral(bin.SecondExpression, isUnionAll);
                }

                if (node is ScalarSubquery q)
                {
                    // TODO : not sure
                    return IsConvertibleToValues(q.QueryExpression, isUnionAll);
                }

                // UNION can be used for var values deduplication
                // thus vars can be converted into VALUES clause in UNION ALL case only
                if (isUnionAll && node is VariableReference)
                {
                    // keep these true-blocks for debugging purpose
                    return true;
                }

                if (node is Literal)
                {
                    // keep these true-blocks for debugging purpose
                    return true;
                }

                return false;
            }

            private static bool AllSelectedElementsAreScalarVarsOrLiterals(IList<SelectElement> selectElements, bool isUnionAll)
            {
                for (int i = selectElements.Count - 1; i >= 0; i--)
                {
                    var selectElement = selectElements[i];

                    if (selectElement is SelectStarExpression)
                    {
                        // SELECT * FROM VALUES is fine
                    }
                    else if (selectElement is SelectScalarExpression expr
                    && IsScalarVarOrLiteral(expr.Expression, isUnionAll))
                    {
                        // SELECT @var or SELECT "literal"
                    }
                    else
                    {
                        // column reference or whatever else than scalar element
                        return false;
                    }
                }

                return true;
            }

            private void ReportNode(TSqlFragment node)
            {
                Callback(node);
                lastReportedTokenIndex = node.LastTokenIndex;
            }
        }
    }
}
