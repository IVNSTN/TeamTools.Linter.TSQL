using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    public class SqlTableDefinition
    {
        public SqlTableDefinition(TableDefinition node, SqlTableType tableType)
        {
            Node = node;
            TableType = tableType;

            if (node.ColumnDefinitions?.Count > 0)
            {
                foreach (var col in ExtractColumns(node.ColumnDefinitions))
                {
                    if (col != null)
                    {
                        Columns.TryAdd(col.Name, col);
                    }
                }
            }
        }

        public IDictionary<string, SqlColumnInfo> Columns { get; } = new Dictionary<string, SqlColumnInfo>(StringComparer.OrdinalIgnoreCase);

        public SqlTableType TableType { get; }

        public TableDefinition Node { get; }

        private static IEnumerable<SqlColumnInfo> ExtractColumns(IList<ColumnDefinition> cols)
        {
            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                yield return SqlColumnInfoBuilder.Make(cols[i]);
            }
        }
    }
}
