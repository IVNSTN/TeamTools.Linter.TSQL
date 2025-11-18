using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0404", "DEPRECATED_DBCC_COMMAND")]
    internal sealed class DeprecatedDbccCommandRule : AbstractRule
    {
        private static readonly HashSet<DbccCommand> DeprecatedDbccCommands = new HashSet<DbccCommand>
        {
            DbccCommand.DBReindex,
            DbccCommand.DBReindexAll,
            DbccCommand.IndexDefrag,
            DbccCommand.ShowContig,
            DbccCommand.PinTable,
            DbccCommand.UnpinTable,
        };

        public DeprecatedDbccCommandRule() : base()
        {
        }

        public override void Visit(DbccStatement node)
        {
            if (!DeprecatedDbccCommands.Contains(node.Command))
            {
                return;
            }

            HandleNodeError(node);
        }
    }
}
