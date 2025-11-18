using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
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

            ValidateColumnAliases(node.SelectElements, ViolationHandlerWithMessage);
        }

        private static string ExtractColumnAlias(SelectScalarExpression col)
        {
            if (col.ColumnName != null)
            {
                return col.ColumnName.Value;
            }
            else if (col.Expression is ColumnReferenceExpression colRef && colRef.MultiPartIdentifier?.Count > 0)
            {
                return colRef.MultiPartIdentifier.GetLastIdentifier().Value;
            }

            return default;
        }

        private static void ValidateColumnAliases(IList<SelectElement> selectedElements, Action<TSqlFragment, string> callback)
        {
            var foundAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int n = selectedElements.Count;
            for (int i = 0; i < n; i++)
            {
                if (selectedElements[i] is SelectScalarExpression exp)
                {
                    string alias = ExtractColumnAlias(exp);
                    if (!string.IsNullOrEmpty(alias)
                    && !foundAliases.Add(alias))
                    {
                        callback(exp, alias);
                    }
                }
            }
        }
    }
}
