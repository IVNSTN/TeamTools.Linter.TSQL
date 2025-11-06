using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0994", "FLOAT_COLUMN")]
    internal sealed class FloatColumnStoredRule : AbstractRule
    {
        private static readonly ICollection<string> FloatNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        static FloatColumnStoredRule()
        {
            FloatNames.Add("FLOAT");
            FloatNames.Add("REAL");
        }

        public FloatColumnStoredRule() : base()
        {
        }

        public override void Visit(TableDefinition node)
        {
            foreach (var col in node.ColumnDefinitions)
            {
                if (col.DataType?.Name != null && FloatNames.Contains(col.DataType.Name.BaseIdentifier.Value))
                {
                    HandleNodeError(col.DataType);
                }
            }
        }
    }
}
