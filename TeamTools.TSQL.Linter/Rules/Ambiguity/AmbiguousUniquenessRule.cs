using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    // TODO : except filtered indexes
    // filtered index applies uniqueness to rows satisfying filter criteria
    [RuleIdentity("AM0702", "AMBIGUOUS_UNIQ")]
    [IndexRule]
    internal sealed class AmbiguousUniquenessRule : AbstractRule
    {
        private static readonly string ColSeparator = ", ";
        private static readonly string ViolationTemplate = "columns {0} already included into {1}";

        public AmbiguousUniquenessRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var elements = new TableDefinitionElementsEnumerator(node);

            // table -> index -> indexed columns
            var found = new Dictionary<string, IDictionary<string, ICollection<string>>>(StringComparer.OrdinalIgnoreCase);

            TableKeyColumnTypeValidator.CheckOnAllTables(
                elements,
                tbl => elements.Indices(tbl)
                    .OfType<SqlIndexInfo>()
                    .Where(idx => idx.IsUnique && !idx.IsColumnStore && idx.Columns.Count > 0)
                    .OrderBy(idx => idx.Columns.Count - (idx.PartitionedOnColumns?.Count ?? 0))
                    .ThenBy(idx => idx.Definition.FirstTokenIndex),
                (tbl, el) => DetectUniqueIndexColumnsIntersection(tbl, el, found));
        }

        private void DetectUniqueIndexColumnsIntersection(
            string tableName,
            SqlTableElement el,
            IDictionary<string, IDictionary<string, ICollection<string>>> found)
        {
            if (el is SqlIndexInfo ii && ii.PartitionedOnColumns?.Count > 0)
            {
                // TODO : not sure how to handle partitioned indices
                // UQ can be non-partitoned and one can legally want to
                // have clustered partitioned index with the same columns indexed
                return;
            }

            var cols = el.Columns.Select(col => col.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (!found.ContainsKey(tableName))
            {
                found.Add(tableName, new Dictionary<string, ICollection<string>>(StringComparer.OrdinalIgnoreCase));
            }

            // TODO : reduce complexity
            var tableIndices = found[tableName];
            foreach (var indexName in tableIndices.Keys)
            {
                var idxCols = tableIndices[indexName];
                var intersection = idxCols
                    .Intersect(cols, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (intersection.Count == idxCols.Count)
                {
                    var details = string.Format(ViolationTemplate, string.Join(ColSeparator, intersection), indexName);
                    HandleNodeError(el.Definition, details);
                }
            }

            if (!found[tableName].ContainsKey(el.Name))
            {
                found[tableName].Add(el.Name, cols);
            }
        }
    }
}
