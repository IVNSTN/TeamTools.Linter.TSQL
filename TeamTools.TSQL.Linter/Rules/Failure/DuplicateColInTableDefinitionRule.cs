using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0987", "DUP_COL_IN_TABLE")]
    internal sealed class DuplicateColInTableDefinitionRule : AbstractRule
    {
        public DuplicateColInTableDefinitionRule() : base()
        {
        }

        public override void Visit(TableDefinition node)
        {
            var colNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in node.ColumnDefinitions)
            {
                string colName = col.ColumnIdentifier.Value;
                if (!colNames.TryAddUnique(colName))
                {
                    HandleNodeError(col, colName);
                }
            }
        }
    }
}
