using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0755", "SMALL_SIZE_COL_NULL")]
    internal sealed class SmallSizeColumnNullableRule : AbstractRule
    {
        private static readonly int MaxStringTypeLength = 6;

        private static readonly Dictionary<string, string> HandledTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { TSqlDomainAttributes.Types.Bit,      TSqlDomainAttributes.Types.Bit },
            { TSqlDomainAttributes.Types.TinyInt,  TSqlDomainAttributes.Types.TinyInt },
            { TSqlDomainAttributes.Types.Varchar,  TSqlDomainAttributes.Types.Varchar },
            { TSqlDomainAttributes.Types.Char,     TSqlDomainAttributes.Types.Char },
            { TSqlDomainAttributes.Types.NVarchar, TSqlDomainAttributes.Types.NVarchar },
            { TSqlDomainAttributes.Types.NChar,    TSqlDomainAttributes.Types.NChar },
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

            if (node.AsFileTable)
            {
                // filetable
                return;
            }

            int n = node.Definition.ColumnDefinitions.Count;
            for (int i = 0; i < n; i++)
            {
                ValidateColumnDefinition(node.Definition.ColumnDefinitions[i]);
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

        private static bool IsCharType(string typeName)
            => typeName.IndexOf("CHAR", StringComparison.OrdinalIgnoreCase) > 0;

        private static bool IsColumnNullable(ColumnDefinition col)
        {
            int n = col.Constraints.Count;
            if (n == 0)
            {
                return true;
            }

            for (int i = 0; i < n; i++)
            {
                var cs = col.Constraints[i];
                if (cs is NullableConstraintDefinition nullConstraint)
                {
                    if (!nullConstraint.Nullable)
                    {
                        return false;
                    }
                }
                else if (cs is UniqueConstraintDefinition uniqueConstraint)
                {
                    // if column list is not empty then this is actually a table-level constraint
                    // mistakenly linked to the last column in table
                    if (uniqueConstraint.IsPrimaryKey && uniqueConstraint.Columns is null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ValidateColumnDefinition(ColumnDefinition col)
        {
            if (col.StorageOptions != null && col.StorageOptions.SparseOption != SparseColumnOption.None)
            {
                // sparse columns are supposed to be nullable
                return;
            }

            if (col.DataType is null)
            {
                // computed col
                return;
            }

            string typeName = col.DataType.GetFullName();

            if (string.IsNullOrEmpty(typeName) || !HandledTypes.TryGetValue(typeName, out var typeSpelling))
            {
                return;
            }

            // Column should not be marked as NOT NULL or PRIMARY KEY
            if (!IsColumnNullable(col))
            {
                return;
            }

            if (IsCharType(typeName))
            {
                if (!IsSupportedLength(col.DataType, out int typeSize))
                {
                    return;
                }

                typeSpelling = $"{typeSpelling}({typeSize})";
            }

            HandleNodeError(col.DataType, typeSpelling);
        }
    }
}
