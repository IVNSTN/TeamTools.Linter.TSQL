using Humanizer;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0222", "ID_FOR_INT")]
    internal sealed class IdSuffixForIntegerTypesOnlyRule : AbstractRule
    {
        private static readonly ICollection<string> SupportedIdTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "INT",
            "BIGINT",
            "SMALLINT",
            "TINYINT",
        };

        public IdSuffixForIntegerTypesOnlyRule() : base()
        {
        }

        public override void Visit(TableDefinition node)
        {
            var colVisitor = new ColumnDefinitionVisitor();
            node.AcceptChildren(colVisitor);

            ValidateColNames(colVisitor.Columns);
        }

        private static bool ValidateColNameForDatatype(string colName, string typeName)
        {
            if (SupportedIdTypes.Contains(typeName))
            {
                return true;
            }

            if (colName.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (colName.EndsWith("_id", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (colName.StartsWith("id_", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // in case if col name is valid and parsable PascalCase, camelCase, snake_case
            // extracting name parts and looking for id in the beginning and at the end
            var nameParts = colName.Humanize().Split(' ');
            if (nameParts.Any())
            {
                if (string.Equals(nameParts.First(), "id", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (string.Equals(nameParts.Last(), "id", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private void ValidateColNames(IDictionary<string, KeyValuePair<string, TSqlFragment>> columns)
        {
            foreach (string colName in columns.Keys)
            {
                if (!ValidateColNameForDatatype(colName, columns[colName].Key))
                {
                    HandleNodeError(columns[colName].Value, string.Format("{0} of type {1}", colName, columns[colName].Key));
                }
            }
        }

        private class ColumnDefinitionVisitor : TSqlFragmentVisitor
        {
            public IDictionary<string, KeyValuePair<string, TSqlFragment>> Columns { get; }
                = new Dictionary<string, KeyValuePair<string, TSqlFragment>>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(ColumnDefinition node)
            {
                if (node.DataType?.Name is null)
                {
                    return;
                }

                Columns.Add(node.ColumnIdentifier.Value, new KeyValuePair<string, TSqlFragment>(node.DataType.Name.BaseIdentifier.Value, node));
            }
        }
    }
}
