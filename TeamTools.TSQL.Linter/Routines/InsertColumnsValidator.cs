using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    internal abstract class InsertColumnsValidator : TSqlFragmentVisitor
    {
        private readonly Dictionary<string, bool> identityInsertState =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public InsertColumnsValidator(Action<TSqlFragment, string> callback)
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
            string tableName = node.SchemaObjectName.GetFullName();

            if (node.Definition is null)
            {
                // e.g. filetable
                return;
            }

            // (temp) table can be dropped and redefined
            TableColumns.Remove(tableName);

            ExtractColDefinitions(tableName, node.Definition.ColumnDefinitions);
        }

        public override void Visit(SetIdentityInsertStatement node)
        {
            string tableName = node.Table.GetFullName();
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
            if (!identityInsertState.ContainsKey(tableName))
            {
                return false;
            }

            return identityInsertState[tableName];
        }

        private void ExtractColDefinitions(string tableName, IList<ColumnDefinition> cols)
        {
            TableColumns.Add(tableName, new Dictionary<string, ColType>(StringComparer.OrdinalIgnoreCase));

            foreach (var col in cols)
            {
                string colName = col.ColumnIdentifier.Value;
                if (TableColumns[tableName].ContainsKey(colName))
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
                else if (col.DataType.Name.BaseIdentifier.Value.ToUpper().In("ROWVERSION", "TIMESTAMP"))
                {
                    colType = ColType.RegularCol;
                }
                else if (col.Constraints.Where(cstr =>
                    (cstr is NullableConstraintDefinition nl && !nl.Nullable)
                    // if columns are defined then PK is a table level constraint, not column level
                    // not treating column as required then
                    || (cstr is UniqueConstraintDefinition uq && uq.IsPrimaryKey && ((uq.Columns?.Count ?? 0) == 0))).Any())
                {
                    colType = ColType.RequiredCol;
                }

                TableColumns[tableName].Add(colName, colType);
            }
        }
    }
}
