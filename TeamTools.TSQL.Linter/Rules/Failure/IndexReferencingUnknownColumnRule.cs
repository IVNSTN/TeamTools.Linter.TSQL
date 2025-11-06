using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0904", "INDEX_REFERS_UNKNOWN_COL")]
    [IndexRule]
    internal sealed class IndexReferencingUnknownColumnRule : AbstractRule
    {
        public IndexReferencingUnknownColumnRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var idxVisitor = new TableIndicesVisitor();
            node.Accept(idxVisitor);

            if (idxVisitor.Indices.Count == 0)
            {
                return;
            }

            ValidateIndexes(idxVisitor);
        }

        private void ValidateIndexes(TableIndicesVisitor idxVisitor)
        {
            List<string> cols = new List<string>();
            cols.AddRange(idxVisitor.Table.Definition.ColumnDefinitions.Select(col => col.ColumnIdentifier.Value));

            foreach (var idx in idxVisitor.Indices)
            {
                var missingCols = idx.Columns.Where(col => !cols.Contains(col.Column.MultiPartIdentifier.Identifiers.Last().Value, StringComparer.OrdinalIgnoreCase))
                    .Select(col => col.Column);
                if (idx.Definition is CreateIndexStatement idxCreate && idxCreate.IncludeColumns.Count > 0)
                {
                    missingCols = missingCols.Union(idxCreate.IncludeColumns.Where(col => !cols.Contains(col.MultiPartIdentifier.Identifiers.Last().Value, StringComparer.OrdinalIgnoreCase)));
                }
                else if (idx.Definition is IndexDefinition idxDef && idxDef.IncludeColumns.Count > 0)
                {
                    missingCols = missingCols.Union(idxDef.IncludeColumns.Where(col => !cols.Contains(col.MultiPartIdentifier.Identifiers.Last().Value, StringComparer.OrdinalIgnoreCase)));
                }

                if (missingCols.Any())
                {
                    HandleNodeError(missingCols.First(), string.Join(",", missingCols.Select(col => col.MultiPartIdentifier.Identifiers.Last().Value).Distinct()));
                }
            }
        }
    }
}
