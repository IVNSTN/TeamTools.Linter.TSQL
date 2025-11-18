using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class TableAliasVisitor : TSqlFragmentVisitor
    {
        private readonly List<Tuple<int, int>> checkedQueries;
        private readonly Dictionary<string, string> aliases;
        private readonly Dictionary<string, string> parentAliases;
        private readonly Action<TSqlFragment, IDictionary<string, string>, string, string> callback;
        private readonly bool autoclearAliases;
        private readonly bool isSubquery = false;
        private int nestLevel = 0;

        public TableAliasVisitor(
            List<Tuple<int, int>> checkedQueries,
            Dictionary<string, string> aliases,
            Action<TSqlFragment, IDictionary<string, string>, string, string> callback,
            bool autoclearAliases = false,
            bool isSubquery = false)
        {
            this.checkedQueries = checkedQueries;
            this.aliases = aliases;
            this.callback = callback;
            this.autoclearAliases = autoclearAliases;
            this.isSubquery = isSubquery;
            if (this.isSubquery)
            {
                // TODO : use one shared object
                this.parentAliases = new Dictionary<string, string>(aliases, StringComparer.OrdinalIgnoreCase);
            }
        }

        public TableAliasVisitor(
            List<Tuple<int, int>> checkedQueries,
            Action<TSqlFragment, IDictionary<string, string>, string, string> callback,
            bool autoclearAliases = true)
        {
            this.checkedQueries = checkedQueries;
            this.aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.callback = callback;
            this.autoclearAliases = autoclearAliases;
        }

        public override void Visit(DeleteSpecification node)
        {
            if (!DoEnterContext(node))
            {
                return;
            }

            ExtractTableAliasesFromReferences(node.FromClause?.TableReferences);
            ExtractTableAliasesFromSubquery(node.WhereClause?.SearchCondition);

            DoExitContext(node);
        }

        public override void ExplicitVisit(UpdateSpecification node)
        {
            if (!DoEnterContext(node))
            {
                return;
            }

            ExtractTableAliasesFromReferences(node.FromClause?.TableReferences);
            ExtractTableAliasesFromSubquery(node.WhereClause?.SearchCondition);
            ExtractTableAliasesFromSubquery(node.SetClauses);

            DoExitContext(node);
        }

        /*
        public override void Visit(ScalarSubquery node)
        {
            if (!DoEnterContext(node))
            {
                return;
            }

            ExtractTableAliasesFromSubquery(node.QueryExpression);

            DoExitContext(node);
        }
        */

        public override void Visit(QuerySpecification node)
        {
            if (!DoEnterContext(node))
            {
                return;
            }

            ExtractTableAliasesFromReferences(node.FromClause?.TableReferences);
            ExtractTableAliasesFromSubquery(node.WhereClause);
            ExtractTableAliasesFromSubquery(node.OrderByClause);
            ExtractTableAliasesFromSubquery(node.SelectElements);

            DoExitContext(node);
        }

        protected void ExtractTableAliasesFromReferences(IList<TableReference> refs)
        {
            if (refs is null)
            {
                return;
            }

            int n = refs.Count;
            for (int i = 0; i < n; i++)
            {
                var tbl = refs[i];

                if (tbl is JoinTableReference j)
                {
                    ExtractTableAliasesFromJoin(j);
                }
                else if (tbl is TableReferenceWithAlias t)
                {
                    ExtractTableAliasesFromSubquery(t);
                    RegisterTableReference(t);
                }
            }
        }

        protected void ExtractTableAliasesFromJoin(JoinTableReference node)
        {
            ExtractTableAliasesFromSubquery(node.FirstTableReference);
            {
                if (node.FirstTableReference is TableReferenceWithAlias t)
                {
                    RegisterTableReference(t);
                }
                else if (node.FirstTableReference is JoinTableReference j)
                {
                    // recursively
                    ExtractTableAliasesFromJoin(j);
                }
            }

            ExtractTableAliasesFromSubquery(node.SecondTableReference);
            {
                if (node.SecondTableReference is TableReferenceWithAlias t)
                {
                    RegisterTableReference(t);
                }
                else if (node.SecondTableReference is JoinTableReference j)
                {
                    // recursively
                    ExtractTableAliasesFromJoin(j);
                }
            }
        }

        protected void ExtractTableAliasesFromSubquery<T>(IList<T> subqueries)
        where T : TSqlFragment
        {
            if (subqueries is null)
            {
                return;
            }

            int n = subqueries.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                ExtractTableAliasesFromSubquery(subqueries[i]);
            }
        }

        protected void ExtractTableAliasesFromSubquery<T>(T subquery)
        where T : TSqlFragment
        {
            if (subquery is null)
            {
                return;
            }

            if ((subquery is QueryDerivedTable qd) && (qd.QueryExpression is BinaryQueryExpression bin))
            {
                GoNested(bin.FirstQueryExpression);
                GoNested(bin.SecondQueryExpression);
            }
            else
            {
                GoNested(subquery);
            }
        }

        protected void RegisterTableReference(TableReferenceWithAlias node)
        {
            string alias = null;
            string name = null;

            if (node.Alias != null)
            {
                alias = node.Alias.Value;
            }

            if (node is NamedTableReference nm)
            {
                if (nm.SchemaObject.Identifiers.Count == 1)
                {
                    name = nm.SchemaObject.Identifiers[0].Value;
                }
                else
                {
                    name = nm.SchemaObject.Identifiers.GetFullName(TSqlDomainAttributes.NamePartSeparator);
                }
            }

            if (string.IsNullOrEmpty(alias))
            {
                if (string.IsNullOrEmpty(name))
                {
                    return;
                }

                alias = name;
            }
            else if (string.IsNullOrEmpty(name))
            {
                name = alias;
            }

            callback?.Invoke(node, aliases, alias, name);

            aliases.TryAdd(alias, name);
        }

        private void GoNested(TSqlFragment subquery)
        {
            if (subquery is null)
            {
                return;
            }

            // TODO : use one shared object
            subquery.Accept(
                new TableAliasVisitor(
                    this.checkedQueries,
                    !autoclearAliases ? this.aliases : new Dictionary<string, string>(this.aliases, StringComparer.OrdinalIgnoreCase),
                    this.callback,
                    autoclearAliases,
                    true));
        }

        private bool DoEnterContext(TSqlFragment node)
        {
            if (checkedQueries.Count > 0)
            {
                var predicate = new Predicate<Tuple<int, int>>(pos =>
                {
                    return pos.Item1 <= node.FirstTokenIndex && node.FirstTokenIndex <= pos.Item2;
                });

                if (checkedQueries.Exists(predicate))
                {
                    // looks like a nested query which was already checked
                    return false;
                }
            }

            nestLevel++;
            return true;
        }

        private void DoExitContext(TSqlFragment node)
        {
            checkedQueries.Add(new Tuple<int, int>(node.FirstTokenIndex, node.LastTokenIndex));
            nestLevel--;
            if (nestLevel == 0 && autoclearAliases)
            {
                aliases.Clear();
                if (isSubquery)
                {
                    foreach (var alias in parentAliases)
                    {
                        aliases.TryAdd(alias.Key, alias.Value);
                    }
                }
            }
        }
    }
}
