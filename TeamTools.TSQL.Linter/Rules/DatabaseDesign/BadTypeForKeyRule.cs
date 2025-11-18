using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0997", "BAD_TYPE_FOR_KEY")]
    internal sealed class BadTypeForKeyRule : ScriptAnalysisServiceConsumingRule
    {
        private static readonly int MaxAllowedStringKey = 257; // double sysname: schema + dot + object
        private static readonly TableKeyColumnTypeValidator TypeValidator;

        private static readonly Dictionary<string, int> AcceptedTypes;
        private static readonly HashSet<string> PartitioningTypes;
        private static readonly HashSet<string> FineAtSecondaryPositions;

        static BadTypeForKeyRule()
        {
            AcceptedTypes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { TSqlDomainAttributes.Types.Bit, default }, // if there is already such a key - why not
                { TSqlDomainAttributes.Types.TinyInt, default },
                { TSqlDomainAttributes.Types.SmallInt, default },
                { TSqlDomainAttributes.Types.Int, default },
                { TSqlDomainAttributes.Types.BigInt, default },
                { "DATE", default },
                { "UNIQUEIDENTIFIER", default },
                { "ROWVERSION", default }, // in history tables - why not
                { "TIMESTAMP", default },
                { TSqlDomainAttributes.Types.Char, MaxAllowedStringKey },
                { TSqlDomainAttributes.Types.Varchar, MaxAllowedStringKey },
                { TSqlDomainAttributes.Types.NChar, MaxAllowedStringKey },
                { TSqlDomainAttributes.Types.NVarchar, MaxAllowedStringKey },
            };

            // also fine at clustered PK
            // but will be treated as violation in FK
            PartitioningTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DATETIME",
                "SMALLDATETIME",
                "DATETIME2",
                "TIME",
            };

            // applied to temp tables, table variables, table types only
            FineAtSecondaryPositions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DECIMAL",
                "NUMERIC",
                "MONEY",
                "SMALLMONEY",
            };

            TypeValidator = new TableKeyColumnTypeValidator(AcceptedTypes, FineAtSecondaryPositions, PartitioningTypes);
        }

        public BadTypeForKeyRule() : base()
        {
        }

        protected override void ValidateScript(TSqlScript node)
        {
            var elements = GetService<TableDefinitionElementsEnumerator>(node);

            if (elements.Tables.Count == 0)
            {
                return;
            }

            TypeValidator.CheckOnAllTables(
                elements,
                tbl => elements.Keys(tbl),
                ViolationHandlerWithMessage);
        }
    }
}
