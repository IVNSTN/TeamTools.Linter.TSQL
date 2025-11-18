using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Routines
{
    // TODO : support temp table name reused scenarios
    // TODO : support both
    //          a) single CREATE TABLE script with multiple batches all about this table
    //          b) not CREATE TABLE script where batch end breaks/clears context
    //          thus table variables disappear, temp tables most likely loose context as well
    // TODO : support DROP TABLE for temp tables - break context... somehow.
    public class TableDefinitionElementsEnumerator
    {
        private readonly Dictionary<string, SqlTableDefinition> tables
            = new Dictionary<string, SqlTableDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, List<SqlTableElement>> tableElements
            = new Dictionary<string, List<SqlTableElement>>(StringComparer.OrdinalIgnoreCase);

        private readonly Action<string, SqlTableDefinition> regTable;
        private readonly Action<string, IList<ColumnDefinition>> regCol;
        private readonly Action<SqlTableElement> regConstraint;
        private readonly Action<string, string> regUid;
        private readonly Action<SqlTableElement> regIdx;

        public TableDefinitionElementsEnumerator(TSqlScript node) : this()
        {
            node.Accept(new TableDefinitionDetector(regTable, regCol));

            node.Accept(new TableConstraintsDetector(regConstraint, regUid));

            node.Accept(new TableIndexDetector(regIdx));
        }

        private TableDefinitionElementsEnumerator()
        {
            regTable = (tableName, def) => tables.TryAdd(tableName, def);
            regCol = (tableName, cols) => AppendColsToTable(tableName, cols);

            regConstraint = cst =>
                {
                    if (!tableElements.TryGetValue(cst.TableName, out var tblElements))
                    {
                        tblElements = new List<SqlTableElement>();
                        tableElements.Add(cst.TableName, tblElements);
                    }

                    tblElements.Add(cst);
                };

            regUid = (tbl, col) =>
                {
                    if (tables.TryGetValue(tbl, out var tblInfo) && tblInfo.Columns.TryGetValue(col, out var colInfo))
                    {
                        colInfo.IsNewId = true;
                    }
                };

            regIdx = idx =>
                {
                    tableElements.TryAdd(idx.TableName, new List<SqlTableElement>());
                    tableElements[idx.TableName].Add(idx);
                };
        }

        public IDictionary<string, SqlTableDefinition> Tables => tables;

        // DF
        public IEnumerable<SqlTableElement> Defaults(string tableName = null)
        {
            return GetFilteredTableElements(
                GetTableNameEnumeration(tableName),
                e => e.ElementType == SqlTableElementType.DefaultConstraint);
        }

        // PK + UQ CS + ALL IX
        public IEnumerable<SqlTableElement> Indices(string tableName = null)
        {
            return GetFilteredTableElements(
                GetTableNameEnumeration(tableName),
                e => e.ElementType == SqlTableElementType.Index
                        || e.ElementType == SqlTableElementType.PrimaryKey
                        || e.ElementType == SqlTableElementType.UniqueConstraint);
        }

        // PK + UQ CS + UQ IX
        public IEnumerable<SqlTableElement> UniqueConstraints(string tableName = null)
        {
            return GetFilteredTableElements(
                GetTableNameEnumeration(tableName),
                e => e.ElementType == SqlTableElementType.UniqueConstraint
                        || e.ElementType == SqlTableElementType.PrimaryKey
                        || (e is SqlIndexInfo ii && ii.IsUnique));
        }

        // FK + PK
        public IEnumerable<SqlTableElement> Keys(string tableName = null)
        {
            return GetFilteredTableElements(
                GetTableNameEnumeration(tableName),
                e => e.ElementType == SqlTableElementType.ForeignKey
                        || e.ElementType == SqlTableElementType.PrimaryKey);
        }

        private IEnumerable<string> GetTableNameEnumeration(string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                if (!tables.ContainsKey(tableName))
                {
                    // no such table
                    return Enumerable.Empty<string>();
                }

                return Enumerable.Repeat(tableName, 1);
            }

            return tables.Keys;
        }

        private IEnumerable<SqlTableElement> GetFilteredTableElements(IEnumerable<string> tableList, Func<SqlTableElement, bool> filter)
        {
            foreach (var tbl in tableList)
            {
                if (!tableElements.TryGetValue(tbl, out var tblElements))
                {
                    continue;
                }

                for (int i = 0, n = tblElements.Count; i < n; i++)
                {
                    var el = tblElements[i];
                    if (filter(el))
                    {
                        yield return el;
                    }
                }
            }
        }

        private void AppendColsToTable(string tableName, IList<ColumnDefinition> cols)
        {
            if (!tables.TryGetValue(tableName, out var tblInfo))
            {
                return;
            }

            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                var col = cols[i];
                tblInfo.Columns.TryAdd(col.ColumnIdentifier.Value, SqlColumnInfoBuilder.Make(col));
            }
        }
    }
}
