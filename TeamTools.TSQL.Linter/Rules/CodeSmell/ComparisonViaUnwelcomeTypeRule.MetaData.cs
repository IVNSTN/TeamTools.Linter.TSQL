using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ComparisonViaUnwelcomeTypeRule : ISqlServerMetadataConsumer
    {
        private SqlServerMetadata metaData;
        private Dictionary<string, string> knownReturnTypes;

        public void LoadMetadata(SqlServerMetadata data)
        {
            metaData = data;

            // Add global variable types
            knownReturnTypes = new Dictionary<string, string>(metaData.GlobalVariables, StringComparer.OrdinalIgnoreCase);

            // Add built-in function types
            foreach (var fn in metaData.Functions)
            {
                if (!string.IsNullOrEmpty(fn.Value.DataType))
                {
                    knownReturnTypes.Add(fn.Key, fn.Value.DataType);
                }
            }
        }
    }
}
