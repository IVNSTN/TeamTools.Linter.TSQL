using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    // TODO : consider migrating to TableDefinitionElementsEnumerator + some insert-only related code
    internal abstract class InsertColumnsValidator : TSqlFragmentVisitor
    {
        private static readonly HashSet<string> TimeStampColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ROWVERSION",
            "TIMESTAMP",
        };

        private Dictionary<string, bool> identityInsertState;

        protected InsertColumnsValidator(Action<TSqlFragment, string> callback)
        {
            Callback = callback;
        }

        protected enum ColType
        {
            /// <summary>
            /// Nullable cols and cols with defaults which can be omitted in insert
            /// </summary>
            RegularCol,

            /// <summary>
            /// IDENTITY col which is illegal in INSERT if IDENTITY_INSERT is ON
            /// </summary>
            IdentityCol,

            /// <summary>
            /// read-only computed col
            /// </summary>
            ComputedCol,

            /// <summary>
            /// NOT NULL with no DEFAULT
            /// </summary>
            RequiredCol,
        }

        protected Action<TSqlFragment, string> Callback { get; }

        protected Dictionary<string, Dictionary<string, ColType>> TableColumns { get; } =
            new Dictionary<string, Dictionary<string, ColType>>(StringComparer.OrdinalIgnoreCase);

        public override void Visit(DeclareTableVariableBody node)
        {
            string tableName = node.VariableName?.Value ?? "RETURN";

            if (TableColumns.ContainsKey(tableName))
            {
                // table var cannot be redefined
                return;
            }

            ExtractColDefinitions(tableName, node.Definition.ColumnDefinitions);
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.AsFileTable)
            {
                // Filetable has no columns
                return;
            }

            string tableName = node.SchemaObjectName.GetFullName();

            // (temp) table can be dropped and redefined
            TableColumns.Remove(tableName);

            ExtractColDefinitions(tableName, node.Definition.ColumnDefinitions);
        }

        public override void Visit(SetIdentityInsertStatement node)
        {
            string tableName = node.Table.GetFullName();

            if (identityInsertState is null)
            {
                identityInsertState = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            }

            if (!identityInsertState.TryAdd(tableName, node.IsOn))
            {
                identityInsertState[tableName] = node.IsOn;
            }
        }

        public override void Visit(InsertSpecification node)
        {
            if (node.Target is null)
            {
                // TODO : support INSERT in MERGE action
                return;
            }

            string targetTable;
            if (node.Target is NamedTableReference tbl)
            {
                targetTable = tbl.SchemaObject.GetFullName();
            }
            else
            {
                return;
            }

            ValidateInsertedColumns(node, targetTable, node.Columns);
        }

        public override void Visit(OutputIntoClause node)
        {
            string targetTable;
            if (node.IntoTable is NamedTableReference tbl)
            {
                targetTable = tbl.SchemaObject.GetFullName();
            }
            else
            {
                return;
            }

            ValidateInsertedColumns(node, targetTable, node.IntoTableColumns);
        }

        protected abstract void ValidateInsertedColumns(TSqlFragment node, string tableName, IList<ColumnReferenceExpression> cols);

        protected bool IsIdentityInsertOnFor(string tableName)
        {
            if (identityInsertState != null && identityInsertState.TryGetValue(tableName, out bool state))
            {
                return state;
            }

            return false;
        }

        private static bool IsRequiredCol(ColumnDefinition col)
        {
            if (col.Constraints.Count == 0)
            {
                return false;
            }

            int n = col.Constraints.Count;
            for (int i = 0; i < n; i++)
            {
                var cstr = col.Constraints[i];
                if (cstr is NullableConstraintDefinition nl && !nl.Nullable)
                {
                    return true;
                }

                if (cstr is UniqueConstraintDefinition uq && uq.IsPrimaryKey)
                {
                    if ((uq.Columns?.Count ?? 0) == 0)
                    {
                        // If columns are defined then the PK is a table level constraint, not column level.
                        // Not treating current column as required then.
                        return true;
                    }
                }
            }

            return false;
        }

        private void ExtractColDefinitions(string tableName, IList<ColumnDefinition> cols)
        {
            var tableCols = new Dictionary<string, ColType>(StringComparer.OrdinalIgnoreCase);
            TableColumns.Add(tableName, tableCols);

            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                var col = cols[i];

                string colName = col.ColumnIdentifier.Value;
                if (tableCols.ContainsKey(colName))
                {
                    // ignoring illegal col dups to prevent "key already exists" error
                    continue;
                }

                ColType colType = ColType.RegularCol;

                if (col.IdentityOptions != null)
                {
                    colType = ColType.IdentityCol;
                }
                else if (col.ComputedColumnExpression != null)
                {
                    colType = ColType.ComputedCol;
                }
                else if (col.DefaultConstraint != null)
                {
                    colType = ColType.RegularCol;
                }
                else if (TimeStampColumns.Contains(col.DataType.Name.BaseIdentifier.Value))
                {
                    colType = ColType.RegularCol;
                }
                else if (IsRequiredCol(col))
                {
                    colType = ColType.RequiredCol;
                }

                tableCols.Add(colName, colType);
            }
        }
    }
}
