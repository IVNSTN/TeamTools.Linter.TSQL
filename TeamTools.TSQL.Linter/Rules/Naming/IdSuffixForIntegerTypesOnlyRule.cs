using Humanizer;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("NM0222", "ID_FOR_INT")]
    internal sealed class IdSuffixForIntegerTypesOnlyRule : AbstractRule
    {
        private static readonly HashSet<string> SupportedIdTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            TSqlDomainAttributes.Types.TinyInt,
            TSqlDomainAttributes.Types.SmallInt,
            TSqlDomainAttributes.Types.Int,
            TSqlDomainAttributes.Types.BigInt,
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
            const string id = "id";
            const string prefixId = "id_";
            const string suffixId = "_id";

            if (SupportedIdTypes.Contains(typeName))
            {
                return true;
            }

            if (colName.Equals(id, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (colName.StartsWith(prefixId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (colName.EndsWith(suffixId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (colName.IndexOf(id, StringComparison.OrdinalIgnoreCase) == -1)
            {
                // further analysis needs 'id' at least somewhere
                return true;
            }

            // in case if col name is valid and parsable PascalCase, camelCase, snake_case
            // extracting name parts and looking for id in the beginning and at the end
            var nameParts = colName.Humanize().Split(' ');
            if (nameParts.Length == 0)
            {
                return true;
            }

            if (string.Equals(nameParts[0], id, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.Equals(nameParts[nameParts.Length - 1], id, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private void ValidateColNames(IDictionary<string, Tuple<string, TSqlFragment>> columns)
        {
            foreach (var col in columns)
            {
                if (!ValidateColNameForDatatype(col.Key, col.Value.Item1))
                {
                    HandleNodeError(col.Value.Item2, string.Format(Strings.ViolationDetails_IdSuffixForIntegerTypesOnlyRule_ColIsOfType, col.Key, col.Value.Item1));
                }
            }
        }

        private class ColumnDefinitionVisitor : TSqlFragmentVisitor
        {
            public IDictionary<string, Tuple<string, TSqlFragment>> Columns { get; }
                = new Dictionary<string, Tuple<string, TSqlFragment>>(StringComparer.OrdinalIgnoreCase);

            public override void Visit(ColumnDefinition node)
            {
                if (node.DataType?.Name is null)
                {
                    return;
                }

                Columns.TryAdd(node.ColumnIdentifier.Value, new Tuple<string, TSqlFragment>(node.DataType.Name.BaseIdentifier.Value, node));
            }
        }
    }
}
