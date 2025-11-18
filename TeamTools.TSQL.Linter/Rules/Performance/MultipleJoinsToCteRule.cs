using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0933", "CTE_MULTIPLE_CALLS")]
    internal sealed class MultipleJoinsToCteRule : AbstractRule
    {
        private const int MaxIssuesPerCte = 3;

        public MultipleJoinsToCteRule() : base()
        {
        }

        public override void Visit(StatementWithCtesAndXmlNamespaces node)
        {
            if ((node.WithCtesAndXmlNamespaces?.CommonTableExpressions?.Count ?? 0) == 0)
            {
                return;
            }

            // TODO : detect refs to CTE from other CTEs
            var cteNames = node.WithCtesAndXmlNamespaces.CommonTableExpressions.Select(cte => cte.ExpressionName.Value);
            var allRefs = ExtractRefs(node);
            var multipleRefs = allRefs
                .Where(cteRef => cteRef.SchemaObject.SchemaIdentifier is null
                    && cteNames.Contains(cteRef.SchemaObject.BaseIdentifier.Value, StringComparer.OrdinalIgnoreCase))
                .GroupBy(cteRef => cteRef.SchemaObject.BaseIdentifier.Value)
                .Select(group => new
                {
                    CteName = group.Key,
                    RefNodes = group.Take(MaxIssuesPerCte),
                    Count = group.Count(),
                })
                .Where(group => group.Count > 1);

            foreach (var mref in multipleRefs)
            {
                foreach (var mnode in mref.RefNodes)
                {
                    HandleNodeError(mnode, mref.CteName);
                }
            }
        }

        private static List<NamedTableReference> ExtractRefs(StatementWithCtesAndXmlNamespaces node)
        {
            var refsFromQueryVisitor = new TableRefVisitor();
            if (node is DeleteStatement del)
            {
                del.DeleteSpecification.Accept(refsFromQueryVisitor);
            }
            else if (node is UpdateStatement upd)
            {
                upd.UpdateSpecification.Accept(refsFromQueryVisitor);
            }
            else if (node is InsertStatement ins)
            {
                ins.InsertSpecification.Accept(refsFromQueryVisitor);
            }
            else if (node is MergeStatement mrg)
            {
                mrg.MergeSpecification.AcceptChildren(refsFromQueryVisitor);
            }
            else if (node is SelectStatement sel)
            {
                sel.QueryExpression.AcceptChildren(refsFromQueryVisitor);
            }

            return refsFromQueryVisitor.Tables;
        }

        private class TableRefVisitor : TSqlFragmentVisitor
        {
            public List<NamedTableReference> Tables { get; } = new List<NamedTableReference>();

            public override void Visit(NamedTableReference node)
            {
                Tables.Add(node);
            }
        }
    }
}
