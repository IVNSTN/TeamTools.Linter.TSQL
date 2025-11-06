using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0770", "SPARSE_UNSUPPORTED_FEATURE")]
    internal sealed class SparseUnsupportedFeatureRule : AbstractRule
    {
        public SparseUnsupportedFeatureRule() : base()
        {
        }

        // TODO : utilize TableDefinitionElementsEnumerator?
        public override void Visit(TableDefinition node)
        {
            if ((node.ColumnDefinitions?.Count ?? 0) == 0)
            {
                // filetable
                return;
            }

            var sparseCols = GetSparseCols(node);

            foreach (var col in sparseCols)
            {
                string colName = col.ColumnIdentifier.Value;

                if (col.DefaultConstraint != null)
                {
                    HandleNodeError(col.DefaultConstraint, $"{colName} cannot have DEFAULT");
                }

                bool nullable = col.Constraints.OfType<NullableConstraintDefinition>().FirstOrDefault()?.Nullable
                    ?? !col.Constraints.OfType<UniqueConstraintDefinition>().Any(uq => uq.IsPrimaryKey);

                if (!nullable)
                {
                    HandleNodeError(col, $"{colName} must allow NULL");
                }

                if (col.IdentityOptions != null)
                {
                    HandleNodeError(col, $"{colName} cannot be IDENTITY");
                }

                if (col.IsRowGuidCol)
                {
                    HandleNodeError(col, $"{colName} cannot be ROWGUIDCOL");
                }

                if (col.StorageOptions?.IsFileStream ?? false)
                {
                    HandleNodeError(col, $"{colName} cannot be FILESTREAM");
                }

                if (col.ComputedColumnExpression != null)
                {
                    // actually this is not a valid syntax
                    HandleNodeError(col, $"{colName} is computed");
                }
            }
        }

        public override void Visit(CreateTableStatement node)
        {
            if ((node.Definition?.ColumnDefinitions?.Count ?? 0) == 0)
            {
                // filetable
                return;
            }

            var compression = node.Options.OfType<TableDataCompressionOption>().FirstOrDefault();

            if (compression is null)
            {
                return;
            }

            var sparseCol = GetSparseCols(node.Definition).FirstOrDefault();
            if (sparseCol != null)
            {
                HandleNodeError(compression, "table cannot be compressed");
            }
        }

        [ExcludeFromCodeCoverage]
        public override void Visit(CreateTypeTableStatement node)
        {
            // Actually parsing of such type definition fails
            var sparseCol = GetSparseCols(node.Definition).FirstOrDefault();

            if (sparseCol != null)
            {
                HandleNodeError(sparseCol, $"table type cannot have sparse cols");
            }
        }

        private static IEnumerable<ColumnDefinition> GetSparseCols(TableDefinition tbl)
        => tbl.ColumnDefinitions.Where(col => col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None);
    }
}
