using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

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
            HashSet<string> connectedRefs,
            IDictionary<string, HashSet<string>> refsBetweenCtes,
            IEnumerable<string> refsToExpand)
        {
            foreach (var cteRef in refsToExpand)
            {
                connectedRefs.Add(cteRef);

                if (refsBetweenCtes.TryGetValue(cteRef, out var cteRefs))
                {
                    foreach (var nestedRef in cteRefs)
                    {
                        if (connectedRefs.Add(nestedRef)
                        && refsBetweenCtes.TryGetValue(nestedRef, out var found))
                        {
                            ExtractTransitiveRefsFromCtes(connectedRefs, refsBetweenCtes, found);
                        }
                    }
                }
            }
        }

        private static IDictionary<string, HashSet<string>> ExtractRefsFromCtes(IList<CommonTableExpression> cteList)
        {
            var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            int n = cteList.Count;
            for (int i = 0; i < n; i++)
            {
                var cte = cteList[i];
                var visitor = new CteRefVisitor();
                cte.QueryExpression.AcceptChildren(visitor);

                if (visitor.FoundRefs.Count != 0)
                {
                    result.Add(cte.ExpressionName.Value, visitor.FoundRefs);
                }
            }

            return result;
        }

        private static HashSet<string> ExtractRefsFromMainQuery(StatementWithCtesAndXmlNamespaces node)
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

        private sealed class CteRefVisitor : TSqlFragmentVisitor
        {
            public HashSet<string> FoundRefs { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
