using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0158", "TABLE_ALL_COL_NULL")]
    internal sealed class TableHasNotNullableColumnsRule : AbstractRule
    {
        public TableHasNotNullableColumnsRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            if ((node.Definition?.ColumnDefinitions.Count ?? 0) == 0)
            {
                // FILETABLE
                return;
            }

            ValidateColumns(node.Definition, node.SchemaObjectName.GetFullName());
        }

        public override void Visit(DeclareTableVariableBody node) => ValidateColumns(node.Definition, node.VariableName?.Value ?? "RESULT");

        private static bool HasNotNullColumn(IList<ColumnDefinition> cols)
        => cols.Any(col =>
            col.Constraints.OfType<NullableConstraintDefinition>().Any(cstr => !cstr.Nullable)
            || col.Constraints.OfType<UniqueConstraintDefinition>().Any(cstr => cstr.IsPrimaryKey && ((cstr.Columns?.Count ?? 0) == 0)));

        private void ValidateColumns(TableDefinition table, string tableName)
        {
            if (!HasNotNullColumn(table.ColumnDefinitions))
            {
                HandleNodeError(table, tableName);
            }
        }
    }
}
