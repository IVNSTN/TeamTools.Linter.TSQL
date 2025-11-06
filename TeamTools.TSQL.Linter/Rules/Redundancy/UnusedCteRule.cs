using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("RD0934", "CTE_UNUSED")]
    internal sealed class UnusedCteRule : AbstractRule
    {
        public UnusedCteRule() : base()
        {
        }

        public override void Visit(StatementWithCtesAndXmlNamespaces node)
        {
            if ((node.WithCtesAndXmlNamespaces?.CommonTableExpressions?.Count ?? 0) == 0)
            {
                return;
            }

            var cteNames = node.WithCtesAndXmlNamespaces.CommonTableExpressions.ToDictionary(cte => cte.ExpressionName.Value, StringComparer.OrdinalIgnoreCase);

            var refsFromQuery = ExtractRefsFromMainQuery(node);

            var refsFromCtes = ExtractRefsFromCtes(node.WithCtesAndXmlNamespaces.CommonTableExpressions);
            if (refsFromCtes.Count > 0)
            {
                var connectedRefsToCte = refsFromCtes
                    .Where(cteRef => refsFromQuery.Contains(cteRef.Key))
                    .SelectMany(cteRef => cteRef.Value)
                    .Distinct();

                ExtractTransitiveRefsFromCtes(
                    refsFromQuery,
                    refsFromCtes,
                    connectedRefsToCte);
            }

            var unusedCtes = cteNames.Where(cte => !refsFromQuery.Contains(cte.Key, StringComparer.OrdinalIgnoreCase));
            foreach (var cte in unusedCtes)
            {
                HandleNodeError(cte.Value.ExpressionName, cte.Key);
            }
        }

        private static void ExtractTransitiveRefsFromCtes(
            ICollection<string> connectedRefs,
            IDictionary<string, IList<string>> refsBetweenCtes,
            IEnumerable<string> refsToExpand)
        {
            foreach (var cteRef in refsToExpand)
            {
                if (!connectedRefs.Contains(cteRef))
                {
                    connectedRefs.Add(cteRef);
                }

                if (!refsBetweenCtes.ContainsKey(cteRef))
                {
                    continue;
                }

                foreach (var nestedRef in refsBetweenCtes[cteRef])
                {
                    if (!connectedRefs.Contains(nestedRef))
                    {
                        connectedRefs.Add(nestedRef);

                        if (refsBetweenCtes.ContainsKey(nestedRef))
                        {
                            ExtractTransitiveRefsFromCtes(connectedRefs, refsBetweenCtes, refsBetweenCtes[nestedRef]);
                        }
                    }
                }
            }
        }

        private static IDictionary<string, IList<string>> ExtractRefsFromCtes(IList<CommonTableExpression> cteList)
        {
            var result = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var cte in cteList)
            {
                var visitor = new CteRefVisitor();
                cte.QueryExpression.AcceptChildren(visitor);

                if (visitor.FoundRefs.Any())
                {
                    result.Add(cte.ExpressionName.Value, visitor.FoundRefs.ToList());
                }
            }

            return result;
        }

        private static List<string> ExtractRefsFromMainQuery(StatementWithCtesAndXmlNamespaces node)
        {
            var refsFromQueryVisitor = new CteRefVisitor();
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

            return refsFromQueryVisitor.FoundRefs;
        }

        private class CteRefVisitor : TSqlFragmentVisitor
        {
            public List<string> FoundRefs { get; } = new List<string>();

            public override void Visit(NamedTableReference node)
            {
                if (node.SchemaObject.SchemaIdentifier != null)
                {
                    return;
                }

                FoundRefs.Add(node.SchemaObject.BaseIdentifier.Value);
            }
        }
    }
}
