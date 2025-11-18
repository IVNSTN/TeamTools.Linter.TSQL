using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Query validator.
    /// </summary>
    internal partial class AmbiguousColumnBelongingRule
    {
        private sealed class QueryValidator : TSqlFragmentVisitor
        {
            private readonly Action<TSqlFragment, string> callback;
            private readonly HashSet<QuerySpecification> analyzedQueries = new HashSet<QuerySpecification>();
            private readonly HashSet<Identifier> reportedNodes = new HashSet<Identifier>();
            private Action<QuerySpecification> anlz;

            public QueryValidator(Action<TSqlFragment, string> callback)
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
                int n = node.SelectElements.Count;
                for (int i = 0; i < n; i++)
                {
                    AnalyzeNestedQueries(node.SelectElements[i], detectNestedQueriesOnlyWithOwnSource);
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
                    int n = upd.SetClauses.Count;
                    for (int i = 0; i < n; i++)
                    {
                        AnalyzeNestedQueries(upd.SetClauses[i], detectNestedQueriesOnlyWithOwnSource);
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

            private static List<MultiPartIdentifier> ExtractColumnRefs(TSqlFragment node)
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
                int result = 0;

                if (from is null)
                {
                    return result;
                }

                int n = from.TableReferences.Count;
                for (int i = 0; i < n; i++)
                {
                    result += GetSourceCount(from.TableReferences[i]);
                }

                return result;
            }

            private void ValidateElements<T>(IList<T> nodes)
            where T : TSqlFragment
            {
                int n = nodes.Count;
                for (int i = 0; i < n; i++)
                {
                    ValidateElements(nodes[i]);
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

            private void ValidateJoinConditions(IList<TableReference> joins)
            {
                int n = joins.Count;
                for (int i = 0; i < n; i++)
                {
                    ValidateJoinConditions(joins[i]);
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
                if (columnRefs.Count == 0)
                {
                    return;
                }

                ValidateColumnAliases(columnRefs);
            }

            private void ValidateColumnAliases(List<MultiPartIdentifier> cols)
            {
                for (int i = 0, n = cols.Count; i < n; i++)
                {
                    var col = cols[i];
                    if (col.Identifiers.Count == 1)
                    {
                        var id = col.Identifiers[0];
                        if (reportedNodes.Add(id))
                        {
                            callback(id, id.Value);
                        }
                    }
                }
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

                var visitor = new NestedQueryVisitor(anlz ?? (anlz = new Action<QuerySpecification>(AnalyzeQuery)), detectOnlyWithFrom);
                node.AcceptChildren(visitor);
            }
        }
    }
}
