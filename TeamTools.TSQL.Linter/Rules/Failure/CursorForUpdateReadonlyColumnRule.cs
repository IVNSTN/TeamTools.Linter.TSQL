using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0981", "CURSOR_MODIFIES_READONLY_COL")]
    internal sealed class CursorForUpdateReadonlyColumnRule : AbstractRule
    {
        public CursorForUpdateReadonlyColumnRule() : base()
        {
        }

        // TODO : Check if UPDATE statements try to modify readonly columns
        public override void Visit(CursorDefinition node)
        {
            if (!(node.Select.QueryExpression.ForClause is UpdateForClause upd
            && (upd.Columns?.Count ?? 0) > 0))
            {
                // no FOR UPDATE or column list omitted
                return;
            }

            var forUpdateOfCols = upd.Columns
                .Where(col => col.MultiPartIdentifier != null)
                .Select(col => col.MultiPartIdentifier.Identifiers.Last().Value)
                .ToDictionary(col => col, col => true, StringComparer.OrdinalIgnoreCase);

            var query = node.Select.QueryExpression.GetQuerySpecification();
            if (query is null)
            {
                return;
            }

            var readonlyCols = query.SelectElements
                .OfType<SelectScalarExpression>()
                // columns with aliases
                .Where(sel => !string.IsNullOrEmpty(sel.ColumnName?.Value))
                // mentioned in FOR UPDATE clause
                .Where(sel => forUpdateOfCols.ContainsKey(sel.ColumnName.Value))
                // but computed
                .Where(sel => IsVariableOrExpression(sel.Expression))
                .ToDictionary(sel => sel.ColumnName.Value, sel => sel.Expression, StringComparer.OrdinalIgnoreCase);

            if (readonlyCols.Any())
            {
                HandleNodeError(
                    readonlyCols.First().Value,
                    string.Join(", ", readonlyCols.Select(_ => _.Key).OrderBy(_ => _)));
            }
        }

        private static bool IsVariableOrExpression(ScalarExpression node)
        {
            while (node is ParenthesisExpression pe)
            {
                node = pe.Expression;
            }

            return !(node is ColumnReferenceExpression);
        }
    }
}
