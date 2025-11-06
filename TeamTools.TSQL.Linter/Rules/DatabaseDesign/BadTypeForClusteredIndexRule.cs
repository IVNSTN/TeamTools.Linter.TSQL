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
    internal sealed class BadTypeForClusteredIndexRule : AbstractRule
    {
        private static readonly int MaxAllowedStringKey = 257; // double sysname: schema + dot + object
        private static readonly TableKeyColumnTypeValidator TypeValidator;

        private static readonly IDictionary<string, int> AcceptedTypes = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static readonly ICollection<string> FineAtSecondaryPositions = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        private static readonly ICollection<string> PartitioningTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        static BadTypeForClusteredIndexRule()
        {
            AcceptedTypes.Add("dbo.TINYINT", default);
            AcceptedTypes.Add("dbo.SMALLINT", default);
            AcceptedTypes.Add("dbo.INT", default);
            AcceptedTypes.Add("dbo.BIGINT", default);
            AcceptedTypes.Add("dbo.DATE", default);
            AcceptedTypes.Add("dbo.DATETIME", default);
            AcceptedTypes.Add("dbo.DATETIME2", default);
            AcceptedTypes.Add("dbo.SMALLDATETIME", default);
            AcceptedTypes.Add("dbo.TIMESTAMP", default);
            AcceptedTypes.Add("dbo.ROWVERSION", default);
            AcceptedTypes.Add("dbo.CHAR", MaxAllowedStringKey);
            AcceptedTypes.Add("dbo.VARCHAR", MaxAllowedStringKey);
            AcceptedTypes.Add("dbo.NCHAR", MaxAllowedStringKey);
            AcceptedTypes.Add("dbo.NVARCHAR", MaxAllowedStringKey);

            // also fine at clustered PK
            // but will be treated as violation in FK
            PartitioningTypes.Add("dbo.DATETIME");
            PartitioningTypes.Add("dbo.SMALLDATETIME");
            PartitioningTypes.Add("dbo.DATETIME2");
            PartitioningTypes.Add("dbo.TIME");

            // applied to temp tables, table variables, table types only
            FineAtSecondaryPositions.Add("dbo.DECIMAL");
            FineAtSecondaryPositions.Add("dbo.NUMERIC");
            FineAtSecondaryPositions.Add("dbo.MONEY");
            FineAtSecondaryPositions.Add("dbo.SMALLMONEY");
            FineAtSecondaryPositions.Add("dbo.BIT");

            TypeValidator = new TableKeyColumnTypeValidator(AcceptedTypes, FineAtSecondaryPositions, PartitioningTypes);
        }

        public BadTypeForClusteredIndexRule() : base()
        {
        }

        public override void Visit(TSqlScript node)
        {
            var elements = new TableDefinitionElementsEnumerator(node);

            TypeValidator.CheckOnAllTables(
                elements,
                tbl => elements.Indices(tbl)
                    .OfType<SqlIndexInfo>()
                    .Where(idx => idx.IsClustered && !idx.IsColumnStore),
                HandleNodeError);
        }
    }
}
