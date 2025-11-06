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
        private readonly IDictionary<string, SqlTableDefinition> tables
            = new SortedDictionary<string, SqlTableDefinition>(StringComparer.OrdinalIgnoreCase);

        private readonly IDictionary<string, ICollection<SqlTableElement>> tableElements
            = new SortedDictionary<string, ICollection<SqlTableElement>>(StringComparer.OrdinalIgnoreCase);

        public TableDefinitionElementsEnumerator(TSqlScript node)
        {
            node.Accept(new TableDefinitionDetector(
                (tableName, def) => tables.TryAdd(tableName, def),
                (tableName, cols) => AppendColsToTable(tableName, cols)));

            node.Accept(new TableConstraintsDetector(
                cst =>
                {
                    if (!tableElements.ContainsKey(cst.TableName))
                    {
                        tableElements.Add(cst.TableName, new List<SqlTableElement>());
                    }

                    tableElements[cst.TableName].Add(cst);
                },
                (tbl, col) =>
                {
                    if (tables.ContainsKey(tbl) && tables[tbl].Columns.ContainsKey(col))
                    {
                        tables[tbl].Columns[col].IsNewId = true;
                    }
                }));

            node.Accept(new TableIndexDetector(
                idx =>
                {
                    if (!tableElements.ContainsKey(idx.TableName))
                    {
                        tableElements.Add(idx.TableName, new List<SqlTableElement>());
                    }

                    tableElements[idx.TableName].Add(idx);
                }));
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

        private ICollection<string> GetTableNameEnumeration(string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                if (!tables.ContainsKey(tableName))
                {
                    // no such table
                    return new List<string>();
                }

                return new List<string> { tableName };
            }

            return tables.Keys;
        }

        private IEnumerable<SqlTableElement> GetFilteredTableElements(IEnumerable<string> tableList, Func<SqlTableElement, bool> filter)
        {
            foreach (var tbl in tableList)
            {
                if (!tableElements.ContainsKey(tbl))
                {
                    continue;
                }

                foreach (var el in tableElements[tbl].Where(t => filter(t)))
                {
                    yield return el;
                }
            }
        }

        private void AppendColsToTable(string tableName, IList<ColumnDefinition> cols)
        {
            if (!tables.ContainsKey(tableName))
            {
                return;
            }

            foreach (var col in cols)
            {
                tables[tableName].Columns.TryAdd(col.ColumnIdentifier.Value, SqlColumnInfoBuilder.Make(col));
            }
        }
    }
}
