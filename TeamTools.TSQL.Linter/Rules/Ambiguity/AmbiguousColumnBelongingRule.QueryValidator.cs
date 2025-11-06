using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Query validator.
    /// </summary>
    internal partial class AmbiguousColumnBelongingRule
    {
        private class QueryValidator : TSqlFragmentVisitor
        {
            private readonly Action<IEnumerable<MultiPartIdentifier>> callback;
            private readonly List<QuerySpecification> analyzedQueries = new List<QuerySpecification>();

            public QueryValidator(Action<IEnumerable<MultiPartIdentifier>> callback)
            {
                this.callback = callback;
            }

            public override void Visit(QuerySpecification node)
            {
                int sourceCount = GetSourceCount(node.FromClause);
                if (sourceCount == 0)
                {
                    return;
                }

                // catch all subqueries in WHERE or SELECT even with 1 source
                // because sources from the outer scope are also visible there
                bool detectNestedQueriesOnlyWithOwnSource = sourceCount == 1;
                foreach (var s in node.SelectElements)
                {
                    AnalyzeNestedQueries(s, detectNestedQueriesOnlyWithOwnSource);
                }

                AnalyzeNestedQueries(node.WhereClause, detectNestedQueriesOnlyWithOwnSource);
                AnalyzeNestedQueries(node.OrderByClause, detectNestedQueriesOnlyWithOwnSource);

                if (sourceCount <= 1)
                {
                    return;
                }

                AnalyzeQuery(node);
            }

            public override void Visit(UpdateDeleteSpecificationBase node)
            {
                int sourceCount = GetSourceCount(node.FromClause);
                if (sourceCount == 0)
                {
                    return;
                }

                // catch all subqueries in WHERE or SELECT even with 1 source
                // because sources from the outer scope are also visible there
                bool detectNestedQueriesOnlyWithOwnSource = sourceCount == 1;
                AnalyzeNestedQueries(node.WhereClause, detectNestedQueriesOnlyWithOwnSource);

                if (node is UpdateSpecification upd)
                {
                    foreach (var s in upd.SetClauses)
                    {
                        AnalyzeNestedQueries(s, detectNestedQueriesOnlyWithOwnSource);
                    }
                }

                if (sourceCount <= 1)
                {
                    return;
                }

                if (node is UpdateSpecification up)
                {
                    ValidateElements(up.SetClauses);
                }

                ValidateElements(node.OutputClause);
                ValidateElements(node.OutputIntoClause);
                ValidateElements(node.WhereClause);
                ValidateElements(node.FromClause);
            }

            public override void Visit(MergeSpecification node)
            {
                // not counting tables on top level because merge is by nature perfomed between 2 "tables"
                ValidateElements(node.SearchCondition);
                ValidateElements(node.ActionClauses);
                ValidateElements(node.OutputClause);
                ValidateElements(node.OutputIntoClause);
                // TableReference will be handled by QueryExpression (if there is a derived query)
            }

            private static IEnumerable<MultiPartIdentifier> ExtractColumnRefs(TSqlFragment node)
            {
                var refVisitor = new ColumnRefVisitor();
                node.Accept(refVisitor);
                return refVisitor.Columns;
            }

            private static int GetSourceCount(TableReference tbl)
            {
                if (tbl is TableReferenceWithAlias)
                {
                    return 1;
                }

                if (tbl is JoinParenthesisTableReference jp)
                {
                    return GetSourceCount(jp.Join);
                }

                if (tbl is JoinTableReference jt)
                {
                    return GetSourceCount(jt.FirstTableReference)
                        + GetSourceCount(jt.SecondTableReference);
                }

                return 0;
            }

            private static int GetSourceCount(FromClause from)
            {
                if (null == from || from.TableReferences.Count == 0)
                {
                    return 0;
                }

                int result = 0;

                foreach (var tblRef in from.TableReferences)
                {
                    result += GetSourceCount(tblRef);
                }

                return result;
            }

            private void ValidateElements(IEnumerable<TSqlFragment> nodes)
            {
                foreach (var node in nodes)
                {
                    ValidateElements(node);
                }
            }

            private void ValidateJoinConditions(TableReference join)
            {
                if (join is QualifiedJoin jq)
                {
                    // search condition still can contain subqueries
                    // and all of them must have columns with aliases for sure
                    ValidateElements(jq.SearchCondition);
                }

                if (join is JoinParenthesisTableReference jp)
                {
                    ValidateJoinConditions(jp.Join);
                }
                else if (join is JoinTableReference jt)
                {
                    ValidateJoinConditions(jt.FirstTableReference);
                    ValidateJoinConditions(jt.SecondTableReference);
                }
            }

            private void ValidateJoinConditions(IEnumerable<TableReference> joins)
            {
                foreach (var join in joins)
                {
                    ValidateJoinConditions(join);
                }
            }

            private void ValidateElements(TSqlFragment node)
            {
                if (node is null)
                {
                    return;
                }

                if (node is FromClause from)
                {
                    // to avoid dup reports on derived queries
                    ValidateJoinConditions(from.TableReferences);

                    return;
                }

                var columnRefs = ExtractColumnRefs(node);
                if (!columnRefs.Any())
                {
                    return;
                }

                ValidateColumnAliases(columnRefs);
            }

            private void ValidateColumnAliases(IEnumerable<MultiPartIdentifier> cols)
            {
                var nonAliasedCols = cols
                    .Where(col => col.Identifiers.Count == 1)
                    .GroupBy(col => col.Identifiers[0].Value)
                    .Select(group => group.First())
                    .ToList();

                if (!nonAliasedCols.Any())
                {
                    return;
                }

                callback(nonAliasedCols);
            }

            private void AnalyzeQuery(QuerySpecification node)
            {
                if (analyzedQueries.Contains(node))
                {
                    return;
                }

                analyzedQueries.Add(node);

                // order of analysis is not required
                // ORDER BY may refer to column aliases thus reporting them is not correct
                // ValidateElements(node.OrderByClause);
                ValidateElements(node.SelectElements);
                ValidateElements(node.HavingClause);
                ValidateElements(node.GroupByClause);
                ValidateElements(node.WhereClause);
                ValidateElements(node.FromClause);
            }

            private void AnalyzeNestedQueries(TSqlFragment node, bool detectOnlyWithFrom)
            {
                if (node is null)
                {
                    return;
                }

                var visitor = new NestedQueryVisitor(q => AnalyzeQuery(q), detectOnlyWithFrom);
                node.AcceptChildren(visitor);
            }
        }
    }
}
