using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

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
            if (node.ColumnDefinitions is null)
            {
                // e.g. filetable
                return;
            }

            var foundNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int n = node.ColumnDefinitions.Count;
            for (int i = 0; i < n; i++)
            {
                var col = node.ColumnDefinitions[i];
                string colName = col.ColumnIdentifier.Value;
                if (!foundNames.Add(colName))
                {
                    HandleNodeError(col, colName);
                }
            }
        }
    }
}
