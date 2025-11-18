using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0829", "FK_ON_TMP")]
    internal sealed class ForeignKeyOnTempTableRule : AbstractRule
    {
        public ForeignKeyOnTempTableRule() : base()
        {
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.AsFileTable)
            {
                // no columns
                return;
            }

            string tableName = node.SchemaObjectName.BaseIdentifier.Value;
            if (!IsTempTable(tableName))
            {
                return;
            }

            ValidateTableDefinition(tableName, node.Definition);
        }

        public override void Visit(AlterTableAddTableElementStatement node)
        {
            var tableName = node.SchemaObjectName.BaseIdentifier.Value;
            if (!IsTempTable(tableName))
            {
                return;
            }

            ValidateTableDefinition(tableName, node.Definition);
        }

        // This will catch temp table as target, not source.
        public override void Visit(ForeignKeyConstraintDefinition node)
        {
            string targetName = node.ReferenceTableName.BaseIdentifier.Value;
            if (!IsTempTable(targetName))
            {
                return;
            }

            HandleNodeError(node.ReferenceTableName, targetName);
        }

        // TODO : extract somewhere for reusability
        private static bool IsTempTable(string name) => name.StartsWith(TSqlDomainAttributes.TempTablePrefix);

        private void ValidateTableDefinition(string tableName, TableDefinition definition)
        {
            if (definition?.ColumnDefinitions is null)
            {
                // e.g. filetable
                return;
            }

            ValidateConstraints(tableName, definition.TableConstraints);
            ValidateColumns(tableName, definition.ColumnDefinitions);
        }

        private void ValidateConstraints(string tableName, IList<ConstraintDefinition> constraints)
        {
            int n = constraints.Count;
            for (int i = 0; i < n; i++)
            {
                if (constraints[i] is ForeignKeyConstraintDefinition fk)
                {
                    HandleNodeError(fk, fk.ConstraintIdentifier?.Value ?? tableName);
                }
            }
        }

        private void ValidateColumns(string tableName, IList<ColumnDefinition> columns)
        {
            for (int i = columns.Count - 1; i >= 0; i--)
            {
                var col = columns[i];
                if (col.Constraints.Count > 0)
                {
                    ValidateConstraints(tableName, col.Constraints);
                }
            }
        }
    }
}
