using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    [ExcludeFromCodeCoverage]
    public class SqlColumnInfo
    {
        public SqlColumnInfo(
            ColumnDefinition node,
            string columnName,
            SqlColumnTypeInfo typeInfo,
            bool isNullable = true,
            bool isIdentity = false,
            bool isNewId = false,
            bool isSparse = false)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Name = columnName;
            IsIdentity = isIdentity;
            IsNewId = isNewId;
            IsNullable = isNullable;
            DataType = typeInfo;
            IsSparse = isSparse;
        }

        public SqlColumnInfo(
            ColumnDefinition node,
            string columnName,
            string typeName,
            int typeSize = -1,
            bool isUserDefined = false,
            bool isNullable = true,
            bool isIdentity = false,
            bool isNewId = false,
            bool isSparse = false)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));

            if (string.IsNullOrEmpty(columnName) || string.IsNullOrEmpty(typeName))
            {
                Debug.Assert(!string.IsNullOrEmpty(columnName), "columnName is null");
                Debug.Assert(!string.IsNullOrEmpty(typeName), "column typeName is null");
            }

            Name = columnName;
            IsIdentity = isIdentity;
            IsNewId = isNewId;
            IsNullable = isNullable;
            IsSparse = isSparse;

            DataType = new SqlColumnTypeInfo(typeName, typeSize, isUserDefined);
        }

        public ColumnDefinition Node { get; }

        public string Name { get; }

        public SqlColumnTypeInfo DataType { get; }

        public string TypeName => DataType.TypeName;

        public int TypeSize => DataType.TypeSize;

        public bool IsIdentity { get; }

        public bool IsNewId { get; internal set; }

        public bool IsNullable { get; }

        public bool IsSparse { get; }

        public class SqlColumnTypeInfo
        {
            public SqlColumnTypeInfo(string typeName, int typeSize, bool isUserDefined)
            {
                TypeName = typeName;
                TypeSize = typeSize;
                IsUserDefined = isUserDefined;
            }

            public string TypeName { get; }

            public int TypeSize { get; }

            public bool IsUserDefined { get; }
        }
    }
}
