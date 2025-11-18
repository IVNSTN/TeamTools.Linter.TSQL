using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Routines
{
    public class TableKeyColumnTypeValidator
    {
        private static readonly string MaxSizeLiteral = "MAX";
        private static readonly string TypeWithSizePattern = "{0}({1})";
        private static readonly string TypeViolationTemplate = Strings.ViolationDetails_TableKeyValidation_ColHasBadType;

        private readonly IDictionary<string, int> allowedTypesAnywhere;
        private readonly HashSet<string> allowedTypesAtSecondaryPos;
        private readonly HashSet<string> allowedTypesForPartitioning;

        public TableKeyColumnTypeValidator(
            IDictionary<string, int> allowedTypesAnywhere,
            HashSet<string> allowedTypesAtSecondaryPos,
            HashSet<string> allowedTypesForPartitioning)
        {
            this.allowedTypesAnywhere = allowedTypesAnywhere;
            this.allowedTypesAtSecondaryPos = allowedTypesAtSecondaryPos;
            this.allowedTypesForPartitioning = allowedTypesForPartitioning;
        }

        public static void CheckOnAllTables(
            TableDefinitionElementsEnumerator elements,
            Func<string, IEnumerable<SqlTableElement>> filter,
            Action<string, SqlTableElement> handler)
        {
            foreach (var tbl in elements.Tables.Keys)
            {
                foreach (var key in filter(tbl))
                {
                    handler(tbl, key);
                }
            }
        }

        public void CheckOnAllTables(
            TableDefinitionElementsEnumerator elements,
            Func<string, IEnumerable<SqlTableElement>> filter,
            Action<TSqlFragment, string> violationCallback)
        {
            CheckOnAllTables(
                elements,
                filter,
                (tbl, el) => ValidateTableKeyColumnTypes(elements.Tables[tbl], el, violationCallback));
        }

        public bool ValidateColumnType(SqlColumnInfo col, out string error, bool isSecondaryCol = false, bool isPartitionedCol = false)
        {
            error = col?.TypeName ?? throw new ArgumentNullException(nameof(col));

            if (string.IsNullOrEmpty(col.TypeName))
            {
                // crutch for some broken cases
                Debug.Fail("Col type is null: " + col.Name);

                return true;
            }

            if (isSecondaryCol && allowedTypesAtSecondaryPos.Contains(col.TypeName))
            {
                // type is fine for cols not in first position
                return true;
            }

            if (isPartitionedCol && allowedTypesForPartitioning.Contains(col.TypeName))
            {
                // type is fine for partitioned cols
                return true;
            }

            if (!allowedTypesAnywhere.TryGetValue(col.TypeName, out var allowedSize))
            {
                // type is not allowed
                return false;
            }

            if (allowedSize == 0)
            {
                // no size limit defined - any col size is fine
                return true;
            }

            if (col.TypeSize <= allowedSize)
            {
                // col size fits limit
                return true;
            }

            if (col.TypeSize == int.MaxValue)
            {
                // e.g. VARCHAR(MAX)
                error = string.Format(TypeWithSizePattern, col.TypeName, MaxSizeLiteral);
            }
            else
            {
                error = string.Format(TypeWithSizePattern, col.TypeName, col.TypeSize.ToString());
            }

            return false;
        }

        private static bool IsColumnPartitioned(string columnName, List<SqlColumnReferenceInfo> partitionedColumns)
        {
            if (partitionedColumns is null || partitionedColumns.Count == 0)
            {
                return false;
            }

            int n = partitionedColumns.Count;
            for (int i = 0; i < n; i++)
            {
                if (partitionedColumns[i].Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void ValidateTableKeyColumnTypes(
            SqlTableDefinition table,
            SqlTableElement tableElement,
            Action<TSqlFragment, string> violationCallback)
        {
            var tblCols = table.Columns;

            int n = tableElement.Columns.Count;
            for (int i = 0; i < n; i++)
            {
                var col = tableElement.Columns[i];

                if (!tblCols.TryGetValue(col.Name, out var colInfo))
                {
                    // unresolved reference
                    continue;
                }

                if (colInfo.DataType.IsUserDefined)
                {
                    // we cannot say anything about UDT - maybe it's fine
                    continue;
                }

                bool isPartitionedCol = default;

                if (table.TableType != SqlTableType.Default)
                {
                    // Rows from temp table, table variables and table types
                    // cannot be referenced from other tables
                    // and cannot be partitioned directly
                    // thus date and time columns can be the way to organize
                    // rows in particular order.
                    // Treating this as a valid scenario.
                    isPartitionedCol = true;
                }
                else
                {
                    isPartitionedCol = tableElement is SqlIndexInfo ii
                        && IsColumnPartitioned(col.Name, ii.PartitionedOnColumns);
                }

                if (!ValidateColumnType(colInfo, out string typeViolation, col.Position > 0, isPartitionedCol))
                {
                    violationCallback(col.Reference, string.Format(TypeViolationTemplate, col.Name, typeViolation));
                }
            }
        }
    }
}
