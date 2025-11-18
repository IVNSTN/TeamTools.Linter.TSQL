using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Table definition validation for ImplicitConversionImpossibleRule.
    /// </summary>
    internal partial class ImpossibleVisitor
    {
        private static readonly string DefaultResultTableName = "RESULT";

        public override void Visit(DeclareTableVariableBody node)
        {
            var tableName = node.VariableName?.Value ?? DefaultResultTableName; // for inline table functions
            ValidateTableDefinition(tableName, node.Definition);
        }

        public override void Visit(CreateTableStatement node)
        {
            var tableName = node.SchemaObjectName.GetFullName();
            ValidateTableDefinition(tableName, node.Definition);
        }

        private void ValidateInlineDefaults(string tableName, IList<ColumnDefinition> cols)
        {
            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                var col = cols[i];
                string colType = typeEvaluator.GetColumnType(tableName, col);

                if (col.DefaultConstraint != null)
                {
                    ValidateCanConvertAtoB(col.DefaultConstraint.Expression, colType);
                }

                // TODO: validate CheckConstraintDefinition
            }
        }

        private void ValidateTableLevelDefault(string tableName, IList<ConstraintDefinition> constraints)
        {
            int n = constraints.Count;
            for (int i = 0; i < n; i++)
            {
                var cstr = constraints[i];
                if (cstr is DefaultConstraintDefinition def)
                {
                    // Column default value must be compatible with column type
                    string colType = typeEvaluator.GetColumnType(tableName, def.Column.Value);
                    ValidateCanConvertAtoB(def.Expression, colType);
                }

                // TODO: validate CheckConstraintDefinition
            }
        }

        private void ValidateTableDefinition(string tableName, TableDefinition node)
        {
            if (node?.ColumnDefinitions is null)
            {
                // e.g. filetable
                return;
            }

            // This call also caches column types for use in constraint and insert validation
            ValidateInlineDefaults(tableName, node.ColumnDefinitions);
            ValidateTableLevelDefault(tableName, node.TableConstraints);

            lastVisitedTokenIndex = node.LastTokenIndex;
        }
    }
}
