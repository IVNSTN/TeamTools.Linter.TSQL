using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

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
            if (node.DataType is null)
            {
                // computed col
                return;
            }

            if (node.DataType.GetFullName().Equals(TSqlDomainAttributes.Types.SysName, StringComparison.OrdinalIgnoreCase))
            {
                HandleNodeError(node);
            }
        }
    }
}
