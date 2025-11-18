using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Metadata loader.
    /// </summary>
    internal sealed partial class InvalidTypeForNumericOperationRule : ISqlServerMetadataConsumer
    {
        private static readonly Dictionary<string, string> KnownTypes;

        private Dictionary<string, string> knownReturnTypes;

        static InvalidTypeForNumericOperationRule()
        {
            // TODO : consolidate all the metadata about known built-in functions
            KnownTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "BIT", default },
                { "TINYINT", default },
                { "SMALLINT", default },
                { "INT", default },
                { "BIGINT", default },
                { "NUMERIC", "MATH" },
                { "DECIMAL", "MATH" },
                { "SMALLMONEY", "MATH" },
                { "MONEY", "MATH" },
                { "FLOAT", "MATH" },
                { "REAL", "MATH" },
                { "DATE", "NONE" },
                { "TIME", "NONE" },
                { "DATETIMEOFFSET", "NONE" },
                { "DATETIME2", "NONE" },
                { "SMALLDATETIME", "SUBTRACT" },
                { "DATETIME", "SUBTRACT" },
                { "CHAR", "ADD" },
                { "VARCHAR", "ADD" },
                { "NCHAR", "ADD" },
                { "NVARCHAR", "ADD" },
                { "TEXT", "NONE" },
                { "NTEXT", "NONE" },
                { "IMAGE", "NONE" },
                { "CURSOR", "NONE" },
                { "ROWVERSION", "NONE" },
                { "BINARY", "BITWISE" },
                { "VARBINARY", "BITWISE" },
                { "HIERARCHYID", "NONE" },
                { "UNIQUEIDENTIFIER", "NONE" },
                { "SQL_VARIANT", "MATH" },
                { "XML", "NONE" },
            };
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            knownReturnTypes = new Dictionary<string, string>(data.GlobalVariables, StringComparer.OrdinalIgnoreCase);

            foreach (var fn in data.Functions)
            {
                if (!string.IsNullOrEmpty(fn.Value.DataType))
                {
                    knownReturnTypes.Add(fn.Key, fn.Value.DataType);
                }
            }
        }
    }
}
