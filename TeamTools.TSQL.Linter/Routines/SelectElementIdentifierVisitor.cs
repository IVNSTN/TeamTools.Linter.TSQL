using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class SelectElementIdentifierVisitor : TSqlFragmentVisitor
    {
        private readonly bool ignoreBinarySecondPart;
        private readonly IList<TSqlFragment> ignoredElements = new List<TSqlFragment>();
        private readonly Action<SelectScalarExpression> callback;

        public SelectElementIdentifierVisitor(bool ignoreBinarySecondPart, Action<SelectScalarExpression> callback)
        {
            this.ignoreBinarySecondPart = ignoreBinarySecondPart;
            this.callback = callback;
        }

        public override void Visit(SelectScalarExpression node)
        {
            if (ignoredElements.Contains(node))
            {
                return;
            }

            callback?.Invoke(node);
        }

        public override void Visit(BinaryQueryExpression node)
        {
            if (!ignoreBinarySecondPart)
            {
                return;
            }

            if (node.SecondQueryExpression is BinaryQueryExpression)
            {
                // recursively
                Visit(node);
                return;
            }

            if (!(node.SecondQueryExpression is QuerySpecification))
            {
                return;
            }

            IgnoreQuerySelectedElements(node.SecondQueryExpression);
        }

        public override void Visit(QueryDerivedTable node)
        {
            if (null == node.Columns || node.Columns.Count == 0)
            {
                return;
            }

            // because derived table definition overrides column identifiers
            IgnoreQuerySelectedElements(node.QueryExpression);
        }

        public override void Visit(CommonTableExpression node)
        {
            if (null == node.Columns || node.Columns.Count == 0)
            {
                return;
            }

            // because derived table definition overrides column identifiers
            IgnoreQuerySelectedElements(node.QueryExpression);
        }

        public override void Visit(ScalarSubquery node)
        {
            // exists, order by, set and so on
            if (!(node.QueryExpression is QuerySpecification))
            {
                // tbd
                return;
            }

            IgnoreQuerySelectedElements(node.QueryExpression);
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
            if (node is BinaryQueryExpression be)
            {
                IgnoreQuerySelectedElements(be.FirstQueryExpression);
                IgnoreQuerySelectedElements(be.SecondQueryExpression);
            }
            else if (node is QuerySpecification qs)
            {
                foreach (var el in qs.SelectElements)
                {
                    ignoredElements.Add(el);
                }
            }
        }
    }
}
