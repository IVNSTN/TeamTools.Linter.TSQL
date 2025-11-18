using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : except filtered indexes
    // filtered index applies uniqueness to rows satisfying filter criteria
    [RuleIdentity("AM0702", "AMBIGUOUS_UNIQ")]
    [IndexRule]
    internal sealed class AmbiguousUniquenessRule : ScriptAnalysisServiceConsumingRule
    {
        private static readonly string ColSeparator = ", ";
        private static readonly string ViolationTemplate = Strings.ViolationDetails_AmbiguousUniquenessRule_ColumnAlreadyIncluded;

        public AmbiguousUniquenessRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var elements = GetService<TableDefinitionElementsEnumerator>(node);

            if (elements.Tables.Count == 0)
            {
                return;
            }

            // table -> index -> indexed columns
            var found = new Dictionary<string, Dictionary<string, List<string>>>(StringComparer.OrdinalIgnoreCase);

            // TODO : optimization and refactoring needed
            foreach (var tbl in elements.Tables)
            {
                foreach (var idx in elements.Indices(tbl.Key)
                    .OfType<SqlIndexInfo>()
                    .Where(idx => idx.IsUnique && !idx.IsColumnStore && idx.Columns.Count > 0)
                    .OrderBy(idx => idx.Columns.Count - (idx.PartitionedOnColumns?.Count ?? 0))
                    .ThenBy(idx => idx.Definition.FirstTokenIndex))
                {
                    DetectUniqueIndexColumnsIntersection(tbl.Key, idx, found);
                }
            }
        }

        private static List<string> ExtractColNames(List<SqlColumnReferenceInfo> cols)
        {
            var res = new List<string>(cols.Count);
            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                res.Add(cols[i].Name);
            }

            return res;
        }

        private void DetectUniqueIndexColumnsIntersection(
            string tableName,
            SqlTableElement el,
            Dictionary<string, Dictionary<string, List<string>>> found)
        {
            if (el is SqlIndexInfo ii && ii.PartitionedOnColumns?.Count > 0)
            {
                // TODO : not sure how to handle partitioned indices
                // UQ can be non-partitoned and one can legally want to
                // have clustered partitioned index with the same columns indexed
                return;
            }

            var cols = ExtractColNames(el.Columns);

            if (!found.TryGetValue(tableName, out var tableIndices))
            {
                tableIndices = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
                found.Add(tableName, tableIndices);
            }

            // TODO : reduce complexity
            foreach (var idx in tableIndices)
            {
                var intersection = idx.Value
                    .Intersect(cols, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (intersection.Length == idx.Value.Count)
                {
                    var details = string.Format(ViolationTemplate, string.Join(ColSeparator, intersection), idx.Key);
                    HandleNodeError(el.Definition, details);
                }
            }

            tableIndices.TryAdd(el.Name, cols);
        }
    }
}
