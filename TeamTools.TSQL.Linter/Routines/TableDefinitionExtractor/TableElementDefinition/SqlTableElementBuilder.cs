using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    internal static class SqlTableElementBuilder
    {
        private static readonly string NonameConstraintDefaultName = "Noname";
        private static readonly string InlineConstraintDefaultName = "Inline";
        private static readonly ICollection<IndexTypeKind> ClusteredIndexTypes = new List<IndexTypeKind>();

        static SqlTableElementBuilder()
        {
            ClusteredIndexTypes.Add(IndexTypeKind.Clustered);
            ClusteredIndexTypes.Add(IndexTypeKind.ClusteredColumnStore);
        }

        public static SqlTableElement Make(string tableName, CreateColumnStoreIndexStatement node)
        {
            var cols = node.Columns
                .Select((col, index) => new SqlColumnReferenceInfo(
                    col.MultiPartIdentifier.Identifiers.Last().Value,
                    index,
                    col))
                .ToList();

            var partCols = node.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns
                .Select((col, index) => new SqlColumnReferenceInfo(col.Value, index, col))
                .ToList();

            return new SqlIndexInfo(
                tableName,
                SqlTableElementType.Index,
                node.Name.Value,
                cols,
                node,
                node.Clustered ?? false,
                true,
                false,
                partCols);
        }

        public static SqlTableElement Make(string tableName, CreateIndexStatement node)
        {
            var cols = node.Columns
                .Select((col, index) => new SqlColumnReferenceInfo(
                    col.Column.MultiPartIdentifier.Identifiers.Last().Value,
                    index,
                    col))
                .ToList();

            var partCols = node.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns
                .Select((col, index) => new SqlColumnReferenceInfo(col.Value, index, col))
                .ToList();

            return new SqlIndexInfo(
                tableName,
                SqlTableElementType.Index,
                node.Name.Value,
                cols,
                node,
                node.Clustered ?? false,
                false,
                node.Unique,
                partCols);
        }

        public static SqlTableElement Make(string tableName, IndexDefinition node)
        {
            var cols = node.Columns
                .Select((col, index) => new SqlColumnReferenceInfo(
                    col.Column.MultiPartIdentifier.Identifiers.Last().Value,
                    index,
                    col))
                .ToList();

            var partCols = node.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns
                .Select((col, index) => new SqlColumnReferenceInfo(col.Value, index, col))
                .ToList();

            return new SqlIndexInfo(
                tableName,
                SqlTableElementType.Index,
                node.Name.Value,
                cols,
                node,
                node.IndexType?.IndexTypeKind == IndexTypeKind.Clustered,
                node.IndexType?.IndexTypeKind == IndexTypeKind.ClusteredColumnStore || node.IndexType?.IndexTypeKind == IndexTypeKind.NonClusteredColumnStore,
                node.Unique,
                partCols);
        }

        public static SqlTableElement Make(string tableName, ForeignKeyConstraintDefinition node)
        {
            var cols = node.Columns
                .Select((col, index) => new SqlColumnReferenceInfo(col.Value, index, col))
                .ToList();

            return new SqlTableElement(
                tableName,
                SqlTableElementType.ForeignKey,
                node.ConstraintIdentifier?.Value ?? NonameConstraintDefaultName,
                cols,
                node);
        }

        public static SqlTableElement Make(string tableName, UniqueConstraintDefinition node)
        {
            var cols = node.Columns
                .Select((col, index) => new SqlColumnReferenceInfo(
                    col.Column.MultiPartIdentifier.Identifiers.Last().Value,
                    index,
                    col))
                .ToList();

            var partCols = node.OnFileGroupOrPartitionScheme?.PartitionSchemeColumns
                .Select((col, index) => new SqlColumnReferenceInfo(col.Value, index, col))
                .ToList();

            bool isClustered;

            if (node.Clustered.HasValue)
            {
                isClustered = node.Clustered.Value;
            }
            else if (node.IndexType != null && node.IndexType.IndexTypeKind.HasValue)
            {
                isClustered = ClusteredIndexTypes.Contains(node.IndexType.IndexTypeKind.Value);
            }
            else
            {
                // pk is clustered by default
                isClustered = node.IsPrimaryKey;
            }

            return new SqlIndexInfo(
                tableName,
                node.IsPrimaryKey ? SqlTableElementType.PrimaryKey : SqlTableElementType.UniqueConstraint,
                node.ConstraintIdentifier?.Value ?? NonameConstraintDefaultName,
                cols,
                node,
                isClustered,
                false,
                true,
                partCols);
        }

        public static SqlTableElement Make(string tableName, DefaultConstraintDefinition node)
        {
            return new SqlTableElement(
                tableName,
                SqlTableElementType.DefaultConstraint,
                node.ConstraintIdentifier?.Value ?? NonameConstraintDefaultName,
                new List<SqlColumnReferenceInfo> { new SqlColumnReferenceInfo(node.Column.Value, 0, node.Column) },
                node);
        }

        public static SqlTableElement Make(string tableName, CheckConstraintDefinition node)
        {
            return new SqlTableElement(
                tableName,
                SqlTableElementType.CheckConstraint,
                node.ConstraintIdentifier?.Value ?? NonameConstraintDefaultName,
                new List<SqlColumnReferenceInfo>(), // dummy
                node);
        }

        // for inline constraints
        public static SqlTableElement Make(
            string tableName,
            string columnName,
            TSqlFragment colNode,
            ConstraintDefinition node)
        {
            var cols = new List<SqlColumnReferenceInfo> { new SqlColumnReferenceInfo(columnName, 0, colNode) };

            var constraintType = SqlTableElementType.DefaultConstraint;

            if (node is ForeignKeyConstraintDefinition)
            {
                constraintType = SqlTableElementType.ForeignKey;
            }
            else if (node is CheckConstraintDefinition)
            {
                constraintType = SqlTableElementType.CheckConstraint;
            }
            else if (node is UniqueConstraintDefinition uq)
            {
                return new SqlIndexInfo(
                    tableName,
                    uq.IsPrimaryKey ? SqlTableElementType.PrimaryKey : SqlTableElementType.UniqueConstraint,
                    uq.ConstraintIdentifier?.Value ?? InlineConstraintDefaultName,
                    cols,
                    node,
                    uq.Clustered ?? uq.IsPrimaryKey, // pk is clustered by default
                    false,
                    true,
                    null);
            }

            return new SqlTableElement(
                tableName,
                constraintType,
                node.ConstraintIdentifier?.Value ?? InlineConstraintDefaultName,
                cols,
                node);
        }
    }
}
