using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal sealed class SelectElementIdentifierVisitor : TSqlFragmentVisitor
    {
        private readonly Action<SelectScalarExpression> callback;
        private List<TSqlFragment> ignoredElements;

        public SelectElementIdentifierVisitor(Action<SelectScalarExpression> callback)
        {
            this.callback = callback;
        }

        public override void Visit(QuerySpecification node)
        {
            if (ignoredElements != null && ignoredElements.Contains(node))
            {
                return;
            }

            int n = node.SelectElements.Count;
            for (int i = 0; i < n; i++)
            {
                if (node.SelectElements[i] is SelectScalarExpression sel)
                {
                    callback(sel);
                }
            }
        }

        // Note, it does not catch OutputIntoClause
        public override void Visit(OutputClause node)
        {
            int n = node.SelectColumns.Count;
            for (int i = 0; i < n; i++)
            {
                if (node.SelectColumns[i] is SelectScalarExpression sel)
                {
                    callback(sel);
                }
            }
        }

        // In UNION, EXCEPT, etc. the first query defined output format
        public override void Visit(BinaryQueryExpression node)
        {
            IgnoreQuerySelectedElements(node.SecondQueryExpression);
        }

        public override void Visit(QueryDerivedTable node)
        {
            if (node.Columns?.Count > 0)
            {
                // because derived table definition overrides column identifiers
                IgnoreQuerySelectedElements(node.QueryExpression);
            }
        }

        public override void Visit(CommonTableExpression node)
        {
            if (node.Columns?.Count > 0)
            {
                // because cte output definition overrides column identifiers
                IgnoreQuerySelectedElements(node.QueryExpression);
            }
        }

        // TODO : (select 1) as t (value INT)
        // Exists, order by, set-select and so on
        public override void Visit(ScalarSubquery node)
        {
            if (node.QueryExpression is QuerySpecification)
            {
                IgnoreQuerySelectedElements(node.QueryExpression);
            }
        }

        /* never visited
        public override void Visit(QueryParenthesisExpression node)
        {
            // subqueries have their own scope and they are QueryExpressions by themselves
            if (!(node.QueryExpression is QuerySpecification))
            {
                return;
            }

            IgnoreQuerySelectedElements(node.QueryExpression);
        */

        private void IgnoreQuerySelectedElements(QueryExpression node)
        {
            if (ignoredElements is null)
            {
                ignoredElements = new List<TSqlFragment>();
            }

            ignoredElements.Add(node);

            if (node is BinaryQueryExpression bin)
            {
                IgnoreQuerySelectedElements(bin.FirstQueryExpression);
                IgnoreQuerySelectedElements(bin.SecondQueryExpression);
            }
        }
    }
}
