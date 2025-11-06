using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0111", "VAR_TYPE_LENGTH")]
    internal sealed class VariableSizeTypeUsageRule : AbstractRule
    {
        private static readonly ICollection<string> InspectedTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "VARCHAR",
            "NVARCHAR",
            "CHAR",
            "NCHAR",
            "VARBINARY",
            "BINARY",
            "DECIMAL",
            "NUMERIC",
            "DATETIME2",
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

            string typeName = node.Name.BaseIdentifier.Value;

            if (!InspectedTypes.Contains(typeName))
            {
                return;
            }

            HandleNodeError(node, typeName.ToUpperInvariant());
        }
    }
}
