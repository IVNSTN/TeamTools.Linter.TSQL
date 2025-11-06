using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    internal class TableConstraintsDetector : TSqlFragmentVisitor
    {
        private readonly Action<SqlTableElement> callback;
        private readonly Action<string, string> uidColumnCallback;

        public TableConstraintsDetector(Action<SqlTableElement> callback, Action<string, string> uidColumnCallback)
        {
            this.callback = callback;
            this.uidColumnCallback = uidColumnCallback;
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.Definition is null)
            {
                // filestream
                return;
            }

            string tableName = node.SchemaObjectName.GetFullName();
            ProcessTableDefinition(tableName, node.Definition);
        }

        public override void Visit(DeclareTableVariableBody node)
        {
            if (node.VariableName is null)
            {
                // table-valued function return type definition
                return;
            }

            string tableName = node.VariableName.Value;
            ProcessTableDefinition(tableName, node.Definition);
        }

        public override void Visit(CreateTypeTableStatement node)
        {
            string tableName = node.Name.GetFullName();
            ProcessTableDefinition(tableName, node.Definition);
        }

        public override void Visit(AlterTableAddTableElementStatement node)
        {
            string tableName = node.SchemaObjectName.GetFullName();
            ProcessTableDefinition(tableName, node.Definition);
        }

        // TODO : inline constraints in column definition
        private void ProcessTableDefinition(string tableName, TableDefinition node)
        {
            foreach (var cs in node.TableConstraints)
            {
                if (cs is DefaultConstraintDefinition df)
                {
                    callback(SqlTableElementBuilder.Make(tableName, df));

                    if (SqlColumnInfoBuilder.IsNewGuidExpression(df.Expression))
                    {
                        uidColumnCallback?.Invoke(tableName, df.Column.Value);
                    }
                }
                else if (cs is UniqueConstraintDefinition uq)
                {
                    callback(SqlTableElementBuilder.Make(tableName, uq));
                }
                else if (cs is ForeignKeyConstraintDefinition fk)
                {
                    callback(SqlTableElementBuilder.Make(tableName, fk));
                }
                else if (cs is CheckConstraintDefinition ck)
                {
                    callback(SqlTableElementBuilder.Make(tableName, ck));
                }
                else
                {
                    Debug.Fail(cs.GetType().Name);
                }
            }

            foreach (var col in node.ColumnDefinitions)
            {
                string colName = col.ColumnIdentifier.Value;

                foreach (var cs in col.Constraints)
                {
                    if (cs is DefaultConstraintDefinition df)
                    {
                        callback(SqlTableElementBuilder.Make(tableName, colName, col, df));
                    }
                    else if (cs is UniqueConstraintDefinition uq)
                    {
                        if (uq.Columns != null && uq.Columns.Any())
                        {
                            // table-level constraint pretending to be column-level
                            callback(SqlTableElementBuilder.Make(tableName, uq));
                        }
                        else
                        {
                            callback(SqlTableElementBuilder.Make(tableName, colName, col, uq));
                        }
                    }
                    else if (cs is ForeignKeyConstraintDefinition fk)
                    {
                        callback(SqlTableElementBuilder.Make(tableName, colName, col, fk));
                    }
                    else if (cs is CheckConstraintDefinition ck)
                    {
                        callback(SqlTableElementBuilder.Make(tableName, colName, col, ck));
                    }
                    else if (cs is NullableConstraintDefinition)
                    {
                        // dummy
                    }
                    else
                    {
                        Debug.Fail(cs.GetType().Name);
                    }
                }
            }
        }
    }
}
