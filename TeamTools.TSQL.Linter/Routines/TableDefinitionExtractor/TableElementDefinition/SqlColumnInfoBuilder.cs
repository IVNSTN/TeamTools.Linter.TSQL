using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    internal static class SqlColumnInfoBuilder
    {
        public static readonly int UnknownSizeValue = -1;

        private static readonly string SysNameTypeName = TSqlDomainAttributes.Types.SysName;
        private static readonly string SysNameTypeAlias = TSqlDomainAttributes.Types.NVarchar;
        private static readonly int SysNameTypeSize = 128;

        private static readonly HashSet<string> GuidFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "NEWID",
            "NEWSEQUENTIALID",
        };

        public static SqlColumnInfo Make(ColumnDefinition node)
        {
            bool isComputed = node.ComputedColumnExpression != null;

            if (!isComputed
            && string.IsNullOrEmpty(node.DataType?.Name.BaseIdentifier.Value))
            {
                // e.g. CURSOR - no support for this
                return default;
            }

            bool isIdentity = node.IdentityOptions != null;
            bool isSparse = node.StorageOptions != null && node.StorageOptions.SparseOption != SparseColumnOption.None;
            bool isNewId = node.DefaultConstraint != null && IsNewGuidExpression(node.DefaultConstraint.Expression);
            bool isNullable = true;
            bool isExplicitNullabilityDefined = false;

            int n = node.Constraints.Count;
            for (int i = 0; i < n; i++)
            {
                var constraint = node.Constraints[i];

                // If there is no explicit NULLability defined whilst col is in PK - it would be
                // treated as implicitly NOT NULL column
                if (constraint is NullableConstraintDefinition nullConstraint)
                {
                    isNullable = nullConstraint.Nullable;
                    isExplicitNullabilityDefined = true;
                }
                else if (!isExplicitNullabilityDefined
                && constraint is UniqueConstraintDefinition uk && uk.IsPrimaryKey
                && (uk.Columns?.Count ?? 0) == 0)
                {
                    // ScriptDom bug: if columns are defined then this is a table-level constraint
                    isNullable = false;
                }
                else if (!isNewId && constraint is DefaultConstraintDefinition defaultConstraint)
                {
                    isNewId = IsNewGuidExpression(defaultConstraint.Expression);
                }
            }

            var colType = GetColumnType(node);

            if (string.IsNullOrEmpty(colType.TypeName))
            {
                // something went wrong
                Debug.Fail(node.ColumnIdentifier.Value);
                return default;
            }

            return new SqlColumnInfo(
                node,
                node.ColumnIdentifier.Value,
                colType,
                isNullable,
                isIdentity,
                isNewId,
                isSparse,
                isComputed);
        }

        public static bool IsNewGuidExpression(ScalarExpression node)
        {
            if (node is ParenthesisExpression pe)
            {
                return IsNewGuidExpression(pe.Expression);
            }

            if (node is FunctionCall fn && fn.Parameters.Count == 0)
            {
                return GuidFunctions.Contains(fn.FunctionName.Value);
            }

            return false;
        }

        private static SqlColumnInfo.SqlColumnTypeInfo GetColumnType(ColumnDefinition col)
        {
            int size = UnknownSizeValue;
            bool isUdt = false;

            // Computed column does not have explicitly defined type info
            if (col.ComputedColumnExpression != null)
            {
                // TODO : try to predict result type based on the expression
                return new SqlColumnInfo.SqlColumnTypeInfo("COMPUTED", size, isUdt);
            }

            string typeName = col?.DataType.GetFullName() ?? throw new ArgumentNullException(nameof(col));

            if (col.DataType is SqlDataTypeReference sdt)
            {
                size = GetTypeSize(sdt.Parameters.FirstOrDefault()) ?? 0;
            }
            else if (string.Equals(typeName, SysNameTypeName, StringComparison.OrdinalIgnoreCase))
            {
                typeName = SysNameTypeAlias;
                size = SysNameTypeSize;
            }
            else
            {
                // TODO : respect GEOMETRY, GEOGRAPHY
                isUdt = true;
            }

            return new SqlColumnInfo.SqlColumnTypeInfo(typeName, size, isUdt);
        }

        private static int? GetTypeSize(Literal size)
        {
            if (size is null)
            {
                return default;
            }

            if (size is MaxLiteral)
            {
                return int.MaxValue;
            }

            if (int.TryParse(size.Value, out int sizeValue))
            {
                return sizeValue;
            }

            return default;
        }
    }
}
