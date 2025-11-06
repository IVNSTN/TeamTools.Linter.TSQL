using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("AM0168", "NONUNIQUE_COLUMN_ALIAS")]
    internal sealed class UniqueColumnAliasRule : AbstractRule
    {
        public UniqueColumnAliasRule() : base()
        {
        }

        public override void Visit(QuerySpecification node)
        {
            if (null != node.ForClause && node.ForClause is XmlForClause)
            {
                // ignoring for XML queries because same alias may be required
                return;
            }

            ValidateColumnAliases(node.SelectElements, HandleNodeError);
        }

        private static string ExtractColumnAlias(SelectScalarExpression col)
        {
            if (col.ColumnName != null)
            {
                return col.ColumnName.Value;
            }
            else if (col.Expression is ColumnReferenceExpression colRef && colRef.MultiPartIdentifier?.Count > 0)
            {
                var id = colRef.MultiPartIdentifier.Identifiers;
                return id[id.Count - 1].Value;
            }

            return default;
        }

        private static void ValidateColumnAliases(IList<SelectElement> selectedElements, Action<TSqlFragment, string> callback)
        {
            var columns = selectedElements
                .OfType<SelectScalarExpression>()
                .Select(col => new KeyValuePair<string, TSqlFragment>(ExtractColumnAlias(col), col))
                .Where(col => !string.IsNullOrEmpty(col.Key));

            var names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in columns)
            {
                if (!names.TryAddUnique(col.Key))
                {
                    callback(col.Value, col.Key);
                }
            }
        }
    }
}
