using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    [ExcludeFromCodeCoverage]
    public class SqlIndexInfo : SqlTableElement
    {
        private readonly List<SqlColumnReferenceInfo> columnsWithoutPartitionedOnes;

        public SqlIndexInfo(
            string tableName,
            SqlTableElementType elementType,
            string name,
            List<SqlColumnReferenceInfo> columns,
            TSqlFragment definition,
            bool isClustered,
            bool isColumnStore,
            bool isUnique,
            List<SqlColumnReferenceInfo> partitionedOnColumns)
        : base(tableName, elementType, name, columns, definition)
        {
            IsClustered = isClustered;
            IsColumnStore = isColumnStore;
            IsUnique = isUnique;
            PartitionedOnColumns = partitionedOnColumns;

            if (PartitionedOnColumns is null || PartitionedOnColumns.Count == 0)
            {
                columnsWithoutPartitionedOnes = columns;
            }
            else
            {
                columnsWithoutPartitionedOnes = Columns
                    .Where(col => !PartitionedOnColumns.Any(pc => string.Equals(pc.Name, col.Name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }
        }

        public bool IsClustered { get; }

        public bool IsColumnStore { get; }

        public bool IsUnique { get; }

        public List<SqlColumnReferenceInfo> PartitionedOnColumns { get; }

        public List<SqlColumnReferenceInfo> ColumnsExceptPartitioned => columnsWithoutPartitionedOnes;
    }
}
