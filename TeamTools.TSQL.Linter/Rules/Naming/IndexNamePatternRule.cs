using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Index name validator.
    /// </summary>
    [RuleIdentity("NM0961", "INDEX_NAME_PATTERN")]
    [IndexRule]
    internal sealed partial class IndexNamePatternRule : AbstractRule
    {
        public IndexNamePatternRule() : base()
        {
        }

        private static void ExtractTableRef(SchemaObjectName definition, out string tableSchema, out string tableName)
        {
            tableName = definition.BaseIdentifier.Value;
            tableSchema = definition.SchemaIdentifier?.Value ?? TSqlDomainAttributes.DefaultSchemaName;

            if (tableName.StartsWith(TSqlDomainAttributes.TempTablePrefix))
            {
                // temp tables shouldn't have schema
                tableName = tableName.TrimStart(TSqlDomainAttributes.TempTablePrefixChar);
                tableSchema = "";
            }
        }

        private void ValidateIndexName(Identifier currentName, string expectedName)
        {
            if (!string.Equals(currentName.Value, expectedName, StringComparison.Ordinal))
            {
                HandleNodeError(currentName, $"{currentName.Value} vs {expectedName}");
            }
        }
    }
}
