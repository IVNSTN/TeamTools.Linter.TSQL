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

        private static readonly string SysNameTypeName = "dbo.SYSNAME";
        private static readonly string SysNameTypeAlias = "dbo.NVARCHAR";
        private static readonly int SysNameTypeSize = 128;
        private static readonly ICollection<string> GuidFunctions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        static SqlColumnInfoBuilder()
        {
            GuidFunctions.Add("NEWID");
            GuidFunctions.Add("NEWSEQUENTIALID");
        }

        public static SqlColumnInfo Make(ColumnDefinition node)
        {
            if (string.IsNullOrEmpty(node.DataType?.Name.BaseIdentifier.Value))
            {
                // e.g. CURSOR - no support for this
                return default;
            }

            // TODO : if there is no explicit NULLability defined
            // then check if col is in PK - it would be treated
            // as implicitly NOT NULL column
            bool isNullable = node.Constraints
                .OfType<NullableConstraintDefinition>()
                .FirstOrDefault()?.Nullable ?? true;

            bool isIdentity = node.IdentityOptions != null;

            bool isNewId = node.Constraints
                .OfType<DefaultConstraintDefinition>()
                .Any(df => IsNewGuidExpression(df.Expression))
                || (node.DefaultConstraint != null && IsNewGuidExpression(node.DefaultConstraint.Expression));

            bool isSparse = node.StorageOptions != null
                && node.StorageOptions.SparseOption != SparseColumnOption.None;

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
                isSparse);
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
            if (col is null)
            {
                throw new ArgumentNullException(nameof(col));
            }

            int size = UnknownSizeValue;
            string typeName = col.DataType.Name.GetFullName();
            bool isUdt = false;

            if (col.DataType is SqlDataTypeReference sdt)
            {
                size = GetTypeSize(sdt.Parameters.FirstOrDefault()) ?? 0;
            }
            else if (col.DataType is UserDataTypeReference)
            {
                if (string.Equals(typeName, SysNameTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    typeName = SysNameTypeAlias;
                    size = SysNameTypeSize;
                }
                else
                {
                    isUdt = true;
                }
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
