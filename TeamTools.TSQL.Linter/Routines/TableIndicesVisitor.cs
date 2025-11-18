using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TeamTools.TSQL.Linter.Routines
{
    [Obsolete("Deprecated. Migrate to" + nameof(TableDefinitionElementsEnumerator))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class TableIndicesVisitor : TSqlFragmentVisitor
    {
        private readonly bool ignoreTempTables = true;

        public TableIndicesVisitor()
        {
        }

        public IList<TableIndexInfo> Indices { get; } = new List<TableIndexInfo>();

        public CreateTableStatement Table { get; private set; }

        public FileGroupOrPartitionScheme OnFileGroupOrPartitionScheme { get; private set; }

        public override void Visit(CreateTableStatement node)
        {
            if (ignoreTempTables && node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                return;
            }

            // approximately "somewhere in the beginning" only
            // so for CREATE TABLE scripts only
            // TODO : use MainScriptObject detector
            if (node.FirstTokenIndex > 10 || Table != null)
            {
                return;
            }

            Table = node;
            OnFileGroupOrPartitionScheme = Table.OnFileGroupOrPartitionScheme;
        }

        public override void Visit(CreateIndexStatement node)
        {
            if (Table is null)
            {
                return;
            }

            RegisterIndexInfo(new TableIndexInfo(node));
        }

        public override void Visit(IndexDefinition node)
        {
            if (Table is null)
            {
                return;
            }

            RegisterIndexInfo(new TableIndexInfo(node));
        }

        public override void Visit(UniqueConstraintDefinition node)
        {
            if (Table is null)
            {
                return;
            }

            if (!node.IsPrimaryKey)
            {
                return;
            }

            if (node.Columns.Count == 0)
            {
                // inline column-level constraints are detected by separate method
                return;
            }

            RegisterIndexInfo(new TableIndexInfo(Table.SchemaObjectName, node));
        }

        public override void Visit(ColumnDefinition col)
        {
            if (Table is null)
            {
                return;
            }

            int n = col.Constraints.Count;
            for (int i = 0; i < n; i++)
            {
                if (col.Constraints[i] is UniqueConstraintDefinition uq && uq.Columns.Count == 0)
                {
                    RegisterIndexInfo(new TableIndexInfo(Table.SchemaObjectName, uq, BuildColumnListForIndexInfo(col)));
                }
            }
        }

        private static IList<ColumnWithSortOrder> BuildColumnListForIndexInfo(ColumnDefinition col)
        {
            var indexColumn = new ColumnWithSortOrder
            {
                Column = new ColumnReferenceExpression
                {
                    MultiPartIdentifier = new MultiPartIdentifier(),
                },
            };
            indexColumn.Column.MultiPartIdentifier.Identifiers.Add(col.ColumnIdentifier);

            return new List<ColumnWithSortOrder> { indexColumn };
        }

        private void RegisterIndexInfo(TableIndexInfo idx)
        {
            Indices.Add(idx);

            if ((idx.Clustered == true) && (idx.OnFileGroupOrPartitionScheme != null) && (this.OnFileGroupOrPartitionScheme is null))
            {
                this.OnFileGroupOrPartitionScheme = idx.OnFileGroupOrPartitionScheme;
            }
        }
    }
}
