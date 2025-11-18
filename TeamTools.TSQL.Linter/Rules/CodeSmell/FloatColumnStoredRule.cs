using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0994", "FLOAT_COLUMN")]
    internal sealed class FloatColumnStoredRule : AbstractRule
    {
        private static readonly HashSet<string> FloatNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "FLOAT",
            "REAL",
        };

        public FloatColumnStoredRule() : base()
        {
        }

        public override void Visit(TableDefinition node)
        {
            int n = node.ColumnDefinitions.Count;
            for (int i = 0; i < n; i++)
            {
                var col = node.ColumnDefinitions[i];

                if (col.DataType?.Name != null && FloatNames.Contains(col.DataType.Name.BaseIdentifier.Value))
                {
                    HandleNodeError(col.DataType);
                }
            }
        }
    }
}
