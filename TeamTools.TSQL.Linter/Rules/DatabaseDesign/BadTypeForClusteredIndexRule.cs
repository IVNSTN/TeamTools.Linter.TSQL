using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0998", "BAD_TYPE_FOR_CLUSTERED_IDX")]
    [IndexRule]
    internal sealed class BadTypeForClusteredIndexRule : ScriptAnalysisServiceConsumingRule
    {
        private static readonly int MaxAllowedStringKey = 257; // double sysname: schema + dot + object
        private static readonly TableKeyColumnTypeValidator TypeValidator;

        private static readonly Dictionary<string, int> AcceptedTypes;
        private static readonly HashSet<string> FineAtSecondaryPositions;
        private static readonly HashSet<string> PartitioningTypes;

        static BadTypeForClusteredIndexRule()
        {
            AcceptedTypes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { TSqlDomainAttributes.Types.TinyInt, default },
                { TSqlDomainAttributes.Types.SmallInt, default },
                { TSqlDomainAttributes.Types.Int, default },
                { TSqlDomainAttributes.Types.BigInt, default },
                { "DATE", default },
                { "DATETIME", default },
                { "DATETIME2", default },
                { "SMALLDATETIME", default },
                { "TIMESTAMP", default },
                { "ROWVERSION", default },
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
                TSqlDomainAttributes.Types.Bit,
            };

            TypeValidator = new TableKeyColumnTypeValidator(AcceptedTypes, FineAtSecondaryPositions, PartitioningTypes);
        }

        public BadTypeForClusteredIndexRule() : base()
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
                tbl => elements.Indices(tbl)
                    .OfType<SqlIndexInfo>()
                    .Where(idx => idx.IsClustered && !idx.IsColumnStore),
                ViolationHandlerWithMessage);
        }
    }
}
