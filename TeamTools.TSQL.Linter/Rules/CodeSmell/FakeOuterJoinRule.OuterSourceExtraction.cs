using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class FakeOuterJoinRule
    {
        private static ICollection<string> ExtractOuterSources(IList<TableReference> sources)
        {
            var outerSourceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = sources.Count - 1; i >= 0; i--)
            {
                var source = sources[i];

                // If there are no joins then nothing to extract
                if (source is JoinTableReference)
                {
                    foreach (var outerSource in ExtractOuterSources(source))
                    {
                        outerSourceNames.Add(outerSource);
                    }
                }
            }

            return outerSourceNames;
        }

        private static IEnumerable<string> ExtractOuterSources(TableReference source)
        {
            if (source is QualifiedJoin qj)
            {
                if (qj.QualifiedJoinType == QualifiedJoinType.LeftOuter)
                {
                    if (qj.SecondTableReference is QualifiedJoin nestedJoin)
                    {
                        // LEFT INNER ON ON - both parts of this construction are OUTER sources
                        return ExtractOuterSources(nestedJoin.FirstTableReference)
                            .Union(ExtractOuterSources(nestedJoin.SecondTableReference));
                    }
                    else
                    {
                        return ExtractOuterSourceNames(qj.SecondTableReference);
                    }
                }
                else if (qj.QualifiedJoinType == QualifiedJoinType.RightOuter)
                {
                    return ExtractOuterSourceNames(qj.FirstTableReference);
                }
            }

            if (source is JoinTableReference uj)
            {
                // extracting joins recursively
                if (uj.FirstTableReference is JoinTableReference join1)
                {
                    return ExtractOuterSources(join1);
                }
                else if (uj.SecondTableReference is JoinTableReference join2)
                {
                    return ExtractOuterSources(join2);
                }
            }
            else
            {
                // Not a join but a table reference
                return ExtractOuterSourceNames(source);
            }

            return Enumerable.Empty<string>();
        }

        private static IEnumerable<string> ExtractOuterSourceNames(TableReference sourceReference)
        {
            if (sourceReference is null)
            {
                yield break;
            }

            if (sourceReference is TableReferenceWithAlias aliased && aliased.Alias != null)
            {
                // Outer source may be referenced either via full name or an alias if provided
                yield return aliased.Alias.Value;
            }

            if (sourceReference is NamedTableReference nm)
            {
                yield return nm.SchemaObject.GetFullName();

                if (nm.SchemaObject.SchemaIdentifier is null
                || string.Equals(nm.SchemaObject.SchemaIdentifier.Value, TSqlDomainAttributes.DefaultSchemaName, StringComparison.OrdinalIgnoreCase))
                {
                    // 'dbo.tbl' can be referenced as 'tbl' with schema name omitted
                    yield return nm.SchemaObject.BaseIdentifier.Value;
                }
            }
            else if (sourceReference is VariableTableReference vr)
            {
                yield return vr.Variable.Name;
            }
        }
    }
}
