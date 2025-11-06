using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class TableAliasVisitor : TSqlFragmentVisitor
    {
        private readonly List<KeyValuePair<int, int>> checkedQueries;
        private readonly IDictionary<string, string> aliases;
        private readonly IDictionary<string, string> parentAliases;
        private readonly Action<TSqlFragment, IDictionary<string, string>, string, string> callback;
        private readonly bool autoclearAliases;
        private readonly bool isSubquery = false;
        private int nestLevel = 0;

        public TableAliasVisitor(
            List<KeyValuePair<int, int>> checkedQueries,
            IDictionary<string, string> aliases,
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
                this.parentAliases = new SortedDictionary<string, string>(aliases);
            }
        }

        public TableAliasVisitor(
            List<KeyValuePair<int, int>> checkedQueries,
            Action<TSqlFragment, IDictionary<string, string>, string, string> callback,
            bool autoclearAliases = true)
        {
            this.checkedQueries = checkedQueries;
            this.aliases = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.callback = callback;
            this.autoclearAliases = autoclearAliases;
        }

        public override void Visit(UpdateDeleteSpecificationBase node)
        {
            if (!DoEnterContext(node))
            {
                return;
            }

            ExtractTableAliasesFromReferences(node.FromClause?.TableReferences);
            ExtractTableAliasesFromSubquery(node.WhereClause);
            if (node is UpdateSpecification upd)
            {
                ExtractTableAliasesFromSubquery(upd.SetClauses?.ToArray());
            }

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
            ExtractTableAliasesFromSubquery(node.SelectElements?.ToArray());

            DoExitContext(node);
        }

        protected void ExtractTableAliasesFromReferences(IList<TableReference> refs)
        {
            if (null == refs)
            {
                return;
            }

            foreach (var tbl in refs)
            {
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

        protected void ExtractTableAliasesFromSubquery(TSqlFragment[] subqueries)
        {
            if (null == subqueries)
            {
                return;
            }

            foreach (var subquery in subqueries)
            {
                ExtractTableAliasesFromSubquery(subquery);
            }
        }

        protected void ExtractTableAliasesFromSubquery(TSqlFragment subquery)
        {
            if (null == subquery)
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
            string alias = "";
            string name = "";

            if (node.Alias != null)
            {
                alias = node.Alias.Value;
            }

            if (node is NamedTableReference nm)
            {
                name = string.Join(TSqlDomainAttributes.NamePartSeparator, nm.SchemaObject.Identifiers.Select(i => i.Value));
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

            if (!aliases.ContainsKey(alias))
            {
                aliases.Add(alias, name);
            }
        }

        private void GoNested(TSqlFragment subquery)
        {
            if (null == subquery)
            {
                return;
            }

            // TODO : use one shared object
            subquery.Accept(
                new TableAliasVisitor(
                    this.checkedQueries,
                    !autoclearAliases ? this.aliases : new Dictionary<string, string>(this.aliases),
                    this.callback,
                    autoclearAliases,
                    true));
        }

        private bool DoEnterContext(TSqlFragment node)
        {
            if (checkedQueries.Exists(pos => pos.Key <= node.FirstTokenIndex && node.FirstTokenIndex <= pos.Value))
            {
                // looks like a nested query which was already checked
                return false;
            }

            nestLevel++;
            return true;
        }

        private void DoExitContext(TSqlFragment node)
        {
            checkedQueries.Add(new KeyValuePair<int, int>(node.FirstTokenIndex, node.LastTokenIndex));
            nestLevel--;
            if (nestLevel == 0 && autoclearAliases)
            {
                aliases.Clear();
                if (isSubquery)
                {
                    foreach (var alias in parentAliases)
                    {
                        aliases.Add(alias.Key, alias.Value);
                    }
                }
            }
        }
    }
}
