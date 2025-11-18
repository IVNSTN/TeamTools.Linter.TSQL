using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// ReturnValueTypeMismatchRule metadata.
    /// </summary>
    internal partial class ReturnValueTypeMismatchRule : ISqlServerMetadataConsumer
    {
        private static readonly Dictionary<string, HashSet<string>> ValueCanBeTreatedAs;
        private HashSet<string> sqlserverTypes;
        private Dictionary<string, string> knownFunctionReturnTypes;

        static ReturnValueTypeMismatchRule()
        {
            // Type defined here as Key can be successfully passed to RETURN for any of defined in the Value list types
            // it will be implicitly converted with no data loss.
            // Some combinations (INT->BIT) are supposed to minimize false-positive detections or RETURN 1 and similar cases.
            // Some combinations like DATETIME2(7)->SMALLDATETIME which are perfectly compatible and support implicit conversion
            // were omitted here because data loss might occur.
            // TODO : However some better solution for such cases should be provided. ExpressionEvaluator can check if the value fits given type range.
            ValueCanBeTreatedAs = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "BIT", MakeList("TINYINT", "SMALLINT", "INT", "BIGINT", "DECIMAL", "FLOAT", "MONEY", "SMALLMONEY") },
                { "TINYINT", MakeList("SMALLINT", "INT", "BIGINT", "DECIMAL", "FLOAT", "MONEY", "SMALLMONEY") },
                { "SMALLINT", MakeList("INT", "BIGINT", "DECIMAL", "FLOAT", "MONEY", "SMALLMONEY") },
                // Natural number literals are treated as INT thus registering these cases as compatible
                // to reduce false-positive reports
                { "INT", MakeList("BIGINT", "TINYINT", "SMALLINT", "BIT", "DECIMAL", "FLOAT", "MONEY", "SMALLMONEY") },
                { "SMALLMONEY", MakeList("MONEY", "DECIMAL") },
                { "MONEY", MakeList("DECIMAL") },
                { "DATE", MakeList("DATETIME", "SMALLDATETIME", "DATETIME2", "DATETIMEOFFSET") },
                { "DATETIME", MakeList("DATETIME2", "SMALLDATETIME", "DATETIMEOFFSET") },
                { "SMALLDATETIME", MakeList("DATETIME", "DATETIME2", "DATETIMEOFFSET") },
                { "DATETIME2", MakeList("DATETIME", "SMALLDATETIME", "DATETIMEOFFSET") },
                { "DATETIMEOFFSET", MakeList("DATETIME", "SMALLDATETIME", "DATETIME2") },
                { "BINARY", MakeList("VARBINARY") },
                // Fixed-length strings conversion to variable-length stirng may lead to loss of trailing spaces.
                // However such cases should be detected by a separate rule.
                { "CHAR", MakeList("VARCHAR", "NCHAR", "NVARCHAR", "XML") },
                { "NCHAR", MakeList("NVARCHAR", "XML") },
                // Possible Unicode loss should be handled by NationalSymbolLossRule
                { "NVARCHAR", MakeList("NCHAR", "CHAR", "VARCHAR", "XML") },
                // string literals are treated as VARCHAR during evaluation thus registering these cases
                // as compatible to reduce false-positive reports
                { "VARCHAR", MakeList("CHAR", "NCHAR", "NVARCHAR", "XML") },
            };
        }

        public void LoadMetadata(SqlServerMetadata metaData)
        {
            // Registering known built-in SqlServer function result types
            knownFunctionReturnTypes = metaData.Functions
                .Where(fn => !string.IsNullOrEmpty(fn.Value.DataType))
                .Select(fn => new KeyValuePair<string, string>(fn.Key, fn.Value.DataType))
                .Union(metaData.GlobalVariables)
                .ToDictionary(fn => fn.Key, fn => fn.Value, StringComparer.OrdinalIgnoreCase);

            // Registering known built-in SqlServer types
            sqlserverTypes = new HashSet<string>(metaData.Types.Keys, StringComparer.OrdinalIgnoreCase);

            var typeAliases = metaData.Types
                .Where(t => !string.IsNullOrEmpty(t.Value.AlsoKnownAs))
                .ToArray();

            // Registering type aliases as equals to similar _conventional_ types
            foreach (var type in typeAliases)
            {
                foreach (var compat in ValueCanBeTreatedAs.Values)
                {
                    if (compat.Contains(type.Value.AlsoKnownAs))
                    {
                        // If something is compatible with conventional type name (e.g. BIT->INT)
                        // then it is fully compatibly with any target type's alias as well (e.g. BIT->INTEGER).
                        // Doing this as a separate step to provide full compatibility lists (INT->DECIMAL, DEC, NUMERIC)
                        // for the next iteration where type mapping is being copied for aliases.
                        compat.Add(type.Key);
                    }
                }
            }

            foreach (var type in typeAliases)
            {
                // conventional type is fully compatible with it's aliases (e.g. INT->INTEGER)
                if (ValueCanBeTreatedAs.TryGetValue(type.Value.AlsoKnownAs, out var conventionalTypeCompatibility))
                {
                    conventionalTypeCompatibility.Add(type.Key);
                }
                else
                {
                    ValueCanBeTreatedAs.Add(type.Value.AlsoKnownAs, MakeList(type.Key));
                }

                // and vice versa (e.g. INTEGER->INT)
                ValueCanBeTreatedAs.TryAdd(type.Key, MakeList(type.Value.AlsoKnownAs));
            }
        }

        private static HashSet<string> MakeList(params string[] values) => new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
    }
}
