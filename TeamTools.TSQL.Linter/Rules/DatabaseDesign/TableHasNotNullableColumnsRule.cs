using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
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
            if (node.AsFileTable)
            {
                // FILETABLE
                return;
            }

            ValidateColumns(node.Definition, node.SchemaObjectName.GetFullName());
        }

        public override void Visit(DeclareTableVariableBody node) => ValidateColumns(node.Definition, node.VariableName?.Value ?? "RESULT");

        private static bool HasNotNullColumn(IList<ColumnDefinition> cols)
        {
            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                var col = cols[i];
                int m = col.Constraints.Count;
                for (int j = 0; j < m; j++)
                {
                    if (col.Constraints[j] is NullableConstraintDefinition nc)
                    {
                        if (!nc.Nullable)
                        {
                            return true;
                        }
                    }
                    else if (col.Constraints[j] is UniqueConstraintDefinition uc)
                    {
                        // if there are columns listed then this is actually table-level constraint
                        // mistakenly bound to specific column by parser
                        if (uc.IsPrimaryKey && (uc.Columns?.Count ?? 0) == 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void ValidateColumns(TableDefinition table, string tableName)
        {
            if (!HasNotNullColumn(table.ColumnDefinitions))
            {
                HandleNodeError(table, tableName);
            }
        }
    }
}
