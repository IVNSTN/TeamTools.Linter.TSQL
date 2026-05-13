using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class ColumnNotIncludedInGroupByRule : ISqlServerMetadataConsumer
    {
        private HashSet<string> nonColumnIdentifiers;

        // TODO : load AGGREGATE and WINDOW(?) functions
        public void LoadMetadata(SqlServerMetadata data)
        {
            if (data.Enums.TryGetValue(TSqlDomainAttributes.DateTimePartEnum, out var dateParts))
            {
                nonColumnIdentifiers = new HashSet<string>(dateParts.Select(dtp => dtp.Name), StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
