using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Routines
{
    public class TableKeyColumnTypeValidator
    {
        private static readonly string MaxSizeLiteral = "MAX";
        private static readonly string TypeWithSizePattern = "{0}({1})";
        private static readonly string TypeViolationTemplate = "{0} is {1}";

        private readonly IDictionary<string, int> allowedTypesAnywhere;
        private readonly ICollection<string> allowedTypesAtSecondaryPos;
        private readonly ICollection<string> allowedTypesForPartitioning;

        public TableKeyColumnTypeValidator(
            IDictionary<string, int> allowedTypesAnywhere,
            ICollection<string> allowedTypesAtSecondaryPos,
            ICollection<string> allowedTypesForPartitioning)
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
                var tblCols = elements.Tables[tbl].Columns;

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
            if (col is null)
            {
                throw new ArgumentNullException(nameof(col));
            }

            error = col.TypeName;

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

            if (!allowedTypesAnywhere.ContainsKey(col.TypeName))
            {
                // type is not allowed
                return false;
            }

            if (allowedTypesAnywhere[col.TypeName] == 0)
            {
                // no size limit defined - any col size is fine
                return true;
            }

            if (col.TypeSize <= allowedTypesAnywhere[col.TypeName])
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

        private void ValidateTableKeyColumnTypes(
            SqlTableDefinition table,
            SqlTableElement tableElement,
            Action<TSqlFragment, string> violationCallback)
        {
            var tblCols = table.Columns;

            foreach (var col in tableElement.Columns)
            {
                if (!tblCols.ContainsKey(col.Name))
                {
                    // unresolved reference
                    continue;
                }

                if (tblCols[col.Name].DataType.IsUserDefined)
                {
                    // we cannot say anything about UDT - maybe it's fine
                    continue;
                }

                bool isPartitionedCol = (tableElement is SqlIndexInfo ii)
                    && (ii.PartitionedOnColumns?
                        .Any(pc => string.Equals(col.Name, pc.Name, StringComparison.OrdinalIgnoreCase))
                        ?? false);

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

                if (!ValidateColumnType(tblCols[col.Name], out string typeViolation, col.Position > 0, isPartitionedCol))
                {
                    violationCallback(col.Reference, string.Format(TypeViolationTemplate, col.Name, typeViolation));
                }
            }
        }
    }
}
