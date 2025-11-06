using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    public class SqlTableDefinition
    {
        public SqlTableDefinition(TableDefinition node, SqlTableType tableType)
        {
            Node = node;
            TableType = tableType;

            Columns = node.ColumnDefinitions
                .Select(col => SqlColumnInfoBuilder.Make(col))
                // some extra anti error protection
                .Where(col => col != null && !string.IsNullOrEmpty(col.TypeName))
                .GroupBy(col => col.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToDictionary(col => col.Name, col => col, StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, SqlColumnInfo> Columns { get; }

        public SqlTableType TableType { get; }

        public TableDefinition Node { get; }
    }
}
