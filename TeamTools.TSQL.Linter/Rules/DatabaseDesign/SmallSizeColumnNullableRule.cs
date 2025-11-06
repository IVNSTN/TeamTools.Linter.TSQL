using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0755", "SMALL_SIZE_COL_NULL")]
    internal sealed class SmallSizeColumnNullableRule : AbstractRule
    {
        private static readonly int MaxStringTypeLength = 6;

        private static readonly ICollection<string> HandledTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dbo.BIT",
            "dbo.TINYINT",
            "dbo.VARCHAR",
            "dbo.CHAR",
            "dbo.NVARCHAR",
            "dbo.NCHAR",
        };

        public SmallSizeColumnNullableRule() : base()
        {
        }

        // TODO : ignore columnstore tables
        public override void Visit(CreateTableStatement node)
        {
            if (node.SchemaObjectName.BaseIdentifier.Value.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // # ignored
                return;
            }

            if ((node.Definition?.ColumnDefinitions.Count ?? 0) == 0)
            {
                // filetable
                return;
            }

            foreach (var col in node.Definition.ColumnDefinitions)
            {
                ValidateColumnDefinition(col);
            }
        }

        private static bool IsSupportedLength(DataTypeReference dataType, out int typeSize)
        {
            if (dataType is SqlDataTypeReference sql && sql.Parameters.Count == 1
            && sql.Parameters[0] is IntegerLiteral i && int.TryParse(i.Value, out typeSize))
            {
                return typeSize <= MaxStringTypeLength;
            }

            typeSize = -1;

            return false;
        }

        private void ValidateColumnDefinition(ColumnDefinition col)
        {
            if (col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None)
            {
                // sparse columns are supposed to be nullable
                return;
            }

            // Computed columns don't have type, CURSOR type does not have type name
            string typeName = col.DataType?.Name?.GetFullName();
            string sizeSuffix = "";

            if (string.IsNullOrEmpty(typeName) || !HandledTypes.Contains(typeName))
            {
                return;
            }

            // Column should not be marked as NOT NULL or PRIMARY KEY
            if (col.Constraints.OfType<NullableConstraintDefinition>().Any(cs => !cs.Nullable)
            || col.Constraints.OfType<UniqueConstraintDefinition>().Any(cs => cs.IsPrimaryKey))
            {
                return;
            }

            if (typeName.IndexOf("CHAR", StringComparison.OrdinalIgnoreCase) > 0)
            {
                if (!IsSupportedLength(col.DataType, out int typeSize))
                {
                    return;
                }

                sizeSuffix = $"({typeSize})";
            }

            HandleNodeError(col.DataType, col.DataType.Name.BaseIdentifier.Value.ToUpperInvariant() + sizeSuffix);
        }
    }
}
