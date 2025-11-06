using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Metadata loader.
    /// </summary>
    internal sealed partial class InvalidTypeForNumericOperationRule : ISqlServerMetadataConsumer
    {
        private static readonly IDictionary<string, string> KnownTypes = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private IDictionary<string, string> knownReturnTypes;

        static InvalidTypeForNumericOperationRule()
        {
            // TODO : consolidate all the metadata about known built-in functions
            KnownTypes.Add("BIT", default);
            KnownTypes.Add("TINYINT", default);
            KnownTypes.Add("SMALLINT", default);
            KnownTypes.Add("INT", default);
            KnownTypes.Add("BIGINT", default);
            KnownTypes.Add("NUMERIC", "MATH");
            KnownTypes.Add("DECIMAL", "MATH");
            KnownTypes.Add("SMALLMONEY", "MATH");
            KnownTypes.Add("MONEY", "MATH");
            KnownTypes.Add("FLOAT", "MATH");
            KnownTypes.Add("REAL", "MATH");
            KnownTypes.Add("DATE", "NONE");
            KnownTypes.Add("TIME", "NONE");
            KnownTypes.Add("DATETIMEOFFSET", "NONE");
            KnownTypes.Add("DATETIME2", "NONE");
            KnownTypes.Add("SMALLDATETIME", "SUBTRACT");
            KnownTypes.Add("DATETIME", "SUBTRACT");
            KnownTypes.Add("CHAR", "ADD");
            KnownTypes.Add("VARCHAR", "ADD");
            KnownTypes.Add("NCHAR", "ADD");
            KnownTypes.Add("NVARCHAR", "ADD");
            KnownTypes.Add("TEXT", "NONE");
            KnownTypes.Add("NTEXT", "NONE");
            KnownTypes.Add("IMAGE", "NONE");
            KnownTypes.Add("CURSOR", "NONE");
            KnownTypes.Add("ROWVERSION", "NONE");
            KnownTypes.Add("BINARY", "BITWISE");
            KnownTypes.Add("VARBINARY", "BITWISE");
            KnownTypes.Add("HIERARCHYID", "NONE");
            KnownTypes.Add("UNIQUEIDENTIFIER", "NONE");
            KnownTypes.Add("SQL_VARIANT", "MATH");
            KnownTypes.Add("XML", "NONE");
        }

        public void LoadMetadata(SqlServerMetadata data)
        {
            knownReturnTypes = data.Functions
                .Where(fn => !string.IsNullOrEmpty(fn.Value.DataType))
                .Select(fn => new KeyValuePair<string, string>(fn.Key, fn.Value.DataType))
                .Union(data.GlobalVariables)
                .ToDictionary(fn => fn.Key, fn => fn.Value, StringComparer.OrdinalIgnoreCase);
        }
    }
}
