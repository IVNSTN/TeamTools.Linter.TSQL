using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    [Obsolete("Deprecated. Migrate to" + nameof(TableDefinitionElementsEnumerator))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class TableIndicesVisitor : TSqlFragmentVisitor
    {
        private readonly bool ignoreTempTables = true;

        public TableIndicesVisitor(bool ignoreTempTables = true)
        {
            this.ignoreTempTables = ignoreTempTables;
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
            if (Table == null)
            {
                return;
            }

            var idx = new TableIndexInfo(node);
            Indices.Add(idx);

            if ((idx.Clustered == true) && (idx.OnFileGroupOrPartitionScheme != null) && (null == this.OnFileGroupOrPartitionScheme))
            {
                this.OnFileGroupOrPartitionScheme = idx.OnFileGroupOrPartitionScheme;
            }
        }

        public override void Visit(IndexDefinition node)
        {
            if (Table == null)
            {
                return;
            }

            var idx = new TableIndexInfo(node);
            Indices.Add(idx);

            if ((idx.Clustered == true) && (idx.OnFileGroupOrPartitionScheme != null) && (null == this.OnFileGroupOrPartitionScheme))
            {
                this.OnFileGroupOrPartitionScheme = idx.OnFileGroupOrPartitionScheme;
            }
        }

        public override void Visit(UniqueConstraintDefinition node)
        {
            if (Table == null)
            {
                return;
            }

            if (!node.IsPrimaryKey)
            {
                return;
            }

            TableIndexInfo idx;
            if (node.Columns.Count == 0)
            {
                var keyColumn = Table.Definition.ColumnDefinitions.FirstOrDefault(col => col.Constraints.Contains(node));
                var cols = new List<ColumnWithSortOrder>();
                if (keyColumn != null)
                {
                    var indexColumn = new ColumnWithSortOrder
                    {
                        Column = new ColumnReferenceExpression
                        {
                            MultiPartIdentifier = new MultiPartIdentifier(),
                        },
                    };
                    indexColumn.Column.MultiPartIdentifier.Identifiers.Add(keyColumn.ColumnIdentifier);
                    cols.Add(indexColumn);
                }

                idx = new TableIndexInfo(Table.SchemaObjectName, node, cols);
            }
            else
            {
                idx = new TableIndexInfo(Table.SchemaObjectName, node);
            }

            Indices.Add(idx);

            if ((idx.Clustered == true) && (idx.OnFileGroupOrPartitionScheme != null) && (null == this.OnFileGroupOrPartitionScheme))
            {
                this.OnFileGroupOrPartitionScheme = idx.OnFileGroupOrPartitionScheme;
            }
        }
    }
}
