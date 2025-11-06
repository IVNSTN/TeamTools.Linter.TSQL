using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0147", "SYSNAME_FOR_TABLE_COL")]
    internal sealed class SysnameTypeForTableColRule : AbstractRule
    {
        public SysnameTypeForTableColRule() : base()
        {
        }

        public override void Visit(ColumnDefinition node)
        {
            if (null == node.DataType)
            {
                // computed col
                return;
            }

            if (string.Equals(node.DataType.Name.BaseIdentifier.Value, "SYSNAME", StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node);
            }
        }
    }
}
