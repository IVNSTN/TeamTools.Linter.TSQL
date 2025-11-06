using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class TableIndexInfo
    {
        public TableIndexInfo(CreateIndexStatement tsql) : base()
        {
            Name = tsql.Name;
            Columns = tsql.Columns;
            OnName = tsql.OnName;
            Unique = tsql.Unique;
            Clustered = tsql.Clustered;
            OnFileGroupOrPartitionScheme = tsql.OnFileGroupOrPartitionScheme;
            IndexOptions = tsql.IndexOptions;
            Definition = tsql;
        }

        public TableIndexInfo(SchemaObjectName table, UniqueConstraintDefinition tsql) : base()
        {
            Name = tsql.ConstraintIdentifier;
            Columns = tsql.Columns;
            OnName = table;
            Unique = tsql.IsPrimaryKey;
            Clustered = tsql.Clustered ?? tsql.IsPrimaryKey; // PK is treated clustered by default
            OnFileGroupOrPartitionScheme = tsql.OnFileGroupOrPartitionScheme;
            IndexOptions = tsql.IndexOptions;
            Definition = tsql;
        }

        public TableIndexInfo(SchemaObjectName table, UniqueConstraintDefinition tsql, IList<ColumnWithSortOrder> cols) : base()
        {
            Name = tsql.ConstraintIdentifier;
            Columns = cols;
            OnName = table;
            Unique = tsql.IsPrimaryKey;
            Clustered = tsql.Clustered ?? tsql.IsPrimaryKey; // PK is treated clustered by default
            OnFileGroupOrPartitionScheme = tsql.OnFileGroupOrPartitionScheme;
            IndexOptions = tsql.IndexOptions;
            Definition = tsql;
        }

        public TableIndexInfo(IndexDefinition tsql) : base()
        {
            Name = tsql.Name;
            Columns = tsql.Columns;
            OnName = null;
            Unique = tsql.Unique;
            Clustered = tsql.IndexType.IndexTypeKind == IndexTypeKind.Clustered;
            OnFileGroupOrPartitionScheme = tsql.OnFileGroupOrPartitionScheme;
            IndexOptions = tsql.IndexOptions;
            Definition = tsql;
        }

        public Identifier Name { get; }

        public IList<ColumnWithSortOrder> Columns { get; }

        public FileGroupOrPartitionScheme OnFileGroupOrPartitionScheme { get; }

        public SchemaObjectName OnName { get; }

        public IList<IndexOption> IndexOptions { get; }

        public TSqlFragment Definition { get; }

        public bool Unique { get; }

        public bool? Clustered { get; }
    }
}
