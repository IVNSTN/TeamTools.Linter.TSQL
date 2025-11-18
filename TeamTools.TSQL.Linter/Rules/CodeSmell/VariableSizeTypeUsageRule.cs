using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0111", "VAR_TYPE_LENGTH")]
    internal sealed class VariableSizeTypeUsageRule : AbstractRule
    {
        private static readonly Dictionary<string, string> InspectedTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "BINARY",     "BINARY" },
            { "CHAR",       "CHAR" },
            { "DATETIME2",  "DATETIME2" },
            { "DECIMAL",    "DECIMAL" },
            { "NCHAR",      "NCHAR" },
            { "NUMERIC",    "NUMERIC" },
            { "NVARCHAR",   "NVARCHAR" },
            { "VARBINARY",  "VARBINARY" },
            { "VARCHAR",    "VARCHAR" },
        };

        public VariableSizeTypeUsageRule() : base()
        {
        }

        public override void Visit(SqlDataTypeReference node)
        {
            if (node.Name is null)
            {
                // e.g. CURSOR
                return;
            }

            if (node.Parameters.Count > 0)
            {
                // size defined
                return;
            }

            string typeName = node.GetFullName();

            if (!InspectedTypes.TryGetValue(typeName, out var typeSpelling))
            {
                return;
            }

            HandleNodeError(node, typeSpelling);
        }
    }
}
