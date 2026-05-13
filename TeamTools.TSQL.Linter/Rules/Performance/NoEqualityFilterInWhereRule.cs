using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0881", "NO_EQUALITY_FILTER_WHERE")]
    internal sealed class NoEqualityFilterInWhereRule : BaseNoEqualityFilterRule
    {
        public NoEqualityFilterInWhereRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if (node.WhereClause?.SearchCondition is null)
            {
                // No WHERE or WHERE CURRENT OF
                return;
            }

            if ((node.FromClause?.TableReferences?.Count ?? 0) == 0)
            {
                // Ignoring queries with no source
                return;
            }

            if (node.FromClause.TableReferences.Count == 1
            && SourceHasLimitedVolume(node.FromClause.TableReferences[0]))
            {
                // Ignoring trivial queries
                return;
            }

            if (ResultSetIsLimitedByJoinedSource(node.FromClause.TableReferences))
            {
                // If query has INNER JOIN to table var/temp table
                // then probably everything is not that bad
                return;
            }

            // TODO : ignore if INNER JOIN with equality exists
            ValidatePredicate(node.WhereClause.SearchCondition);
        }

        private static bool SourceHasLimitedVolume(TableReference src)
        {
            return src is VariableTableReference
                || src is GlobalFunctionTableReference
                || src is OpenJsonTableReference
                || src is OpenXmlTableReference
                || src is InlineDerivedTable
                || IsTempTable(src);
        }

        private static bool IsTempTable(TableReference tbl)
        {
            if (!(tbl is NamedTableReference name))
            {
                return false;
            }

            string tblName = name.SchemaObject.BaseIdentifier.Value;

            return tblName.StartsWith(TSqlDomainAttributes.TempTablePrefix)
                || TSqlDomainAttributes.IsTriggerSystemTable(tblName);
        }

        private static bool ResultSetIsLimitedByJoinedSource(IList<TableReference> tables)
        {
            for (int i = tables.Count - 1; i >= 0; i--)
            {
                var tbl = tables[i];

                if (tbl is QualifiedJoin j)
                {
                    if (j.QualifiedJoinType == QualifiedJoinType.FullOuter)
                    {
                        // FULL JOIN
                        return false;
                    }

                    if (j.QualifiedJoinType == QualifiedJoinType.Inner
                    && (SourceHasLimitedVolume(j.FirstTableReference) || SourceHasLimitedVolume(j.SecondTableReference))
                    && HasEqualityFilter(j.SearchCondition))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
