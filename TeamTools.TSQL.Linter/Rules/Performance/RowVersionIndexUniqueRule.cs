using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("PF0130", "ROWVERSION_UNIQUE")]
    [IndexRule]
    internal sealed class RowVersionIndexUniqueRule : ScriptAnalysisServiceConsumingRule
    {
        internal static readonly HashSet<string> RequiredTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ROWVERSION",
            "TIMESTAMP",
        };

        public RowVersionIndexUniqueRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var tableVisitor = GetService<TableIndicesVisitor>(node);

            if (tableVisitor.Table is null || tableVisitor.Indices.Count == 0)
            {
                return;
            }

            var columnVisitor = new ColumnVisitor(RequiredTypes);
            tableVisitor.Table.AcceptChildren(columnVisitor);

            if (columnVisitor.Columns.Count == 0)
            {
                return;
            }

            CheckIndices(columnVisitor.Columns, tableVisitor.Indices);
        }

        private static Identifier DetectColumnOfMonitoredTypesInIndex(ICollection<string> badColumns, IList<ColumnWithSortOrder> indexedColumns)
        {
            int n = indexedColumns.Count;
            for (int i = 0; i < n; i++)
            {
                var indexedColName = indexedColumns[i].Column.MultiPartIdentifier.GetLastIdentifier();

                if (badColumns.Contains(indexedColName.Value))
                {
                    return indexedColName;
                }
            }

            return default;
        }

        private void CheckIndices(ICollection<string> columns, IList<TableIndexInfo> indices)
        {
            int n = indices.Count;
            for (int i = 0; i < n; i++)
            {
                var idx = indices[i];

                if (idx.Unique)
                {
                    // Index marked as UNIQUE is fine
                    continue;
                }

                var badCol = DetectColumnOfMonitoredTypesInIndex(columns, idx.Columns);
                HandleNodeErrorIfAny(badCol, badCol?.Value);
            }
        }
    }
}
