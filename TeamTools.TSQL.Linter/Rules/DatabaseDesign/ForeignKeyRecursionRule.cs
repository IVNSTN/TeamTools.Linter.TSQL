using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0906", "FK_RECURSION")]
    internal sealed class ForeignKeyRecursionRule : AbstractRule
    {
        public ForeignKeyRecursionRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node) => ValidateTableDefinition(node.SchemaObjectName.GetFullName(), node.Definition);

        public override void Visit(AlterTableAddTableElementStatement node) => ValidateTableDefinition(node.SchemaObjectName.GetFullName(), node.Definition);

        private void ValidateTableDefinition(string tableName, TableDefinition node)
        {
            if (node?.ColumnDefinitions is null)
            {
                // e.g. filetable
                return;
            }

            ValidateConstraints(tableName, node.TableConstraints);
            ValidateColumns(tableName, node.ColumnDefinitions);
        }

        private void ValidateConstraints(string srcTable, IList<ConstraintDefinition> constraints)
        {
            int n = constraints.Count;
            for (int i = 0; i < n; i++)
            {
                if (constraints[i] is ForeignKeyConstraintDefinition fk)
                {
                    string dstTable = fk.ReferenceTableName.GetFullName();

                    if (dstTable.Equals(srcTable, StringComparison.OrdinalIgnoreCase))
                    {
                        HandleNodeError(constraints[i]);
                    }
                }
            }
        }

        private void ValidateColumns(string srcTable, IList<ColumnDefinition> columns)
        {
            for (int i = columns.Count - 1; i >= 0; i--)
            {
                var col = columns[i];
                if (col.Constraints.Count > 0)
                {
                    ValidateConstraints(srcTable, col.Constraints);
                }
            }
        }
    }
}
