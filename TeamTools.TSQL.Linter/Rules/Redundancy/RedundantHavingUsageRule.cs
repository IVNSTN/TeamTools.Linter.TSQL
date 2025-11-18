using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0193", "REDUNDANT_HAVING")]
    internal sealed class RedundantHavingUsageRule : AbstractRule
    {
        public RedundantHavingUsageRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if (node.HavingClause is null)
            {
                return;
            }

            if (node.GroupByClause is null)
            {
                // we cannot determine groups
                return;
            }

            var groupedCols = new HashSet<string>(
                node.GroupByClause.GroupingSpecifications
                    .OfType<ExpressionGroupingSpecification>()
                    .Select(ex => ex.Expression)
                    .OfType<ColumnReferenceExpression>()
                    .Select(colRef => colRef.MultiPartIdentifier.GetLastIdentifier().Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);

            var havingArguments = ConditionSplitter.Parse(node.HavingClause.SearchCondition);

            for (int i = havingArguments.Count - 1; i >= 0; i--)
            {
                var colRef = havingArguments[i];
                var colName = colRef.MultiPartIdentifier.GetLastIdentifier().Value;
                // TODO : Respect source table aliases. Column with similar name may come from different source.
                if (groupedCols.Contains(colName))
                {
                    HandleNodeError(colRef, colName);
                }
            }
        }

        private static class ConditionSplitter
        {
            // AVG, MIN and such on a grouping key make no sense
            private static readonly HashSet<string> AggregateFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "COUNT",
                "COUNT_BIG",
                "SUM",
            };

            public static List<ColumnReferenceExpression> Parse(BooleanExpression expr)
            {
                var havingArgs = new List<ColumnReferenceExpression>();
                DoExtractExpressionFrom(expr, havingArgs);
                return havingArgs;
            }

            private static bool DoRegisterArgument(ScalarExpression node, List<ColumnReferenceExpression> result)
            {
                while (node is ParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is FunctionCall fe)
                {
                    if (AggregateFunctions.Contains(fe.FunctionName.Value))
                    {
                        // computing aggregated values within a group is fine
                        return false;
                    }

                    return DoRegisterCompositeArgs(result, fe.Parameters.ToArray());
                }
                else if (node is IIfCall iie)
                {
                    if (DoRegisterCompositeArgs(result, iie.ThenExpression, iie.ElseExpression))
                    {
                        DoExtractExpressionFrom(iie.Predicate, result);
                    }
                }
                else if (node is BinaryExpression be)
                {
                    // TODO : shouldn't then be treated as a single complex expression?
                    DoRegisterArgument(be.FirstExpression, result);
                    DoRegisterArgument(be.SecondExpression, result);
                }
                else if (node is UnaryExpression ue)
                {
                    return DoRegisterArgument(ue.Expression, result);
                }
                else if (node is ColumnReferenceExpression colRef && colRef.MultiPartIdentifier != null)
                {
                    result.Add(colRef);
                }

                // If it is a valid expression for HAVING clause but not a column reference
                // then we return 'true' whilst not registering it in result list - only columns are expected in result.
                // TODO : It is actually possible to register all expressions from both GROUP BY and HAVING clauses
                // and compare them later via GetFragmentCleanedText()
                return true;
            }

            private static bool DoRegisterCompositeArgs(List<ColumnReferenceExpression> result, params ScalarExpression[] expressions)
            {
                var compositeExpr = new List<ColumnReferenceExpression>();

                foreach (var e in expressions)
                {
                    // If either of them is legal in HAVING, we don't want any
                    if (!DoRegisterArgument(e, compositeExpr))
                    {
                        return false;
                    }
                }

                result.AddRange(compositeExpr);
                return true;
            }

            private static void DoExtractExpressionFrom(BooleanExpression node, List<ColumnReferenceExpression> result)
            {
                while (node is BooleanParenthesisExpression pe)
                {
                    node = pe.Expression;
                }

                if (node is BooleanBinaryExpression be)
                {
                    // Complex expressions combined with AND, OR are independent expressions
                    DoExtractExpressionFrom(be.FirstExpression, result);
                    DoExtractExpressionFrom(be.SecondExpression, result);
                }
                else if (node is BooleanNotExpression nee)
                {
                    DoExtractExpressionFrom(nee.Expression, result);
                }
                else if (node is BooleanComparisonExpression ce)
                {
                    // If either side of comparison is valid for HAVING
                    // then whole expression is valid
                    DoRegisterCompositeArgs(result, ce.FirstExpression, ce.SecondExpression);
                }
                else if (node is BooleanIsNullExpression ie)
                {
                    DoRegisterArgument(ie.Expression, result);
                }
                else if (node is InPredicate ipre)
                {
                    DoRegisterArgument(ipre.Expression, result);
                }
            }
        }
    }
}
