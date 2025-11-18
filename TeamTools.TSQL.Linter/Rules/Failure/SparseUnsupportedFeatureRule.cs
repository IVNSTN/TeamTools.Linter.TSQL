using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Properties;
using TeamTools.TSQL.Linter.Routines;

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

            int n = node.ColumnDefinitions.Count;
            for (int i = 0; i < n; i++)
            {
                var col = node.ColumnDefinitions[i];

                if (!IsColumnSparse(col))
                {
                    continue;
                }

                string colName = col.ColumnIdentifier.Value;

                if (col.DefaultConstraint != null)
                {
                    HandleNodeError(col.DefaultConstraint, string.Format(Strings.ViolationDetails_SparseUnsupportedFeatureRule_ColCannotHaveDefault, colName));
                }

                if (!IsColumnNullable(col))
                {
                    HandleNodeError(col, string.Format(Strings.ViolationDetails_SparseUnsupportedFeatureRule_ColCannotBeNotNullable, colName));
                }

                if (col.IdentityOptions != null)
                {
                    HandleNodeError(col, string.Format(Strings.ViolationDetails_SparseUnsupportedFeatureRule_ColCannotBeIdentity, colName));
                }

                if (col.IsRowGuidCol)
                {
                    HandleNodeError(col, string.Format(Strings.ViolationDetails_SparseUnsupportedFeatureRule_ColCannotBeRowguidCol, colName));
                }

                if (col.StorageOptions?.IsFileStream ?? false)
                {
                    HandleNodeError(col, string.Format(Strings.ViolationDetails_SparseUnsupportedFeatureRule_ColCannotBeFilestream, colName));
                }

                if (col.ComputedColumnExpression != null)
                {
                    // actually this is not a valid syntax
                    HandleNodeError(col, string.Format(Strings.ViolationDetails_SparseUnsupportedFeatureRule_ColCannotBeComputed, colName));
                }
            }
        }

        public override void Visit(CreateTableStatement node)
        {
            if (node.AsFileTable)
            {
                // filetable has no columns
                return;
            }

            var compression = LocateCompressionOption(node.Options);

            if (compression is null)
            {
                return;
            }

            if (LocateFirstSparseCol(node.Definition.ColumnDefinitions) is null)
            {
                return;
            }

            HandleNodeError(compression, string.Format(Strings.ViolationDetails_SparseUnsupportedFeatureRule_TableCannotBeCompressed, node.SchemaObjectName.GetFullName()));
        }

        [ExcludeFromCodeCoverage]
        public override void Visit(CreateTypeTableStatement node)
        {
            // Actually parsing of such type definition fails
            var sparseCol = LocateFirstSparseCol(node.Definition.ColumnDefinitions);

            HandleNodeErrorIfAny(sparseCol, "table type cannot have sparse cols");
        }

        private static bool IsColumnSparse(ColumnDefinition col)
            => col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None;

        private static bool IsColumnNullable(ColumnDefinition col)
        {
            int n = col.Constraints.Count;
            for (int i = 0; i < n; i++)
            {
                var cs = col.Constraints[i];
                if (cs is NullableConstraintDefinition nullability)
                {
                    return nullability.Nullable;
                }
                else if (cs is UniqueConstraintDefinition uq
                && uq.IsPrimaryKey && (uq.Columns?.Count ?? 0) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static ColumnDefinition LocateFirstSparseCol(IList<ColumnDefinition> cols)
        {
            int n = cols.Count;
            for (int i = 0; i < n; i++)
            {
                var col = cols[i];
                if (IsColumnSparse(col))
                {
                    return col;
                }
            }

            return default;
        }

        private static TableOption LocateCompressionOption(IList<TableOption> options)
        {
            int n = options.Count;
            for (int i = 0; i < n; i++)
            {
                if (options[i] is TableDataCompressionOption cmp)
                {
                    return cmp;
                }
            }

            return default;
        }
    }
}
