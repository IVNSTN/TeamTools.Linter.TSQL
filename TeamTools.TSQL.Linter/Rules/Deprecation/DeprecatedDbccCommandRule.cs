using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0404", "DEPRECATED_DBCC_COMMAND")]
    internal sealed class DeprecatedDbccCommandRule : AbstractRule
    {
        private static readonly Lazy<IList<DbccCommand>> DeprecatedDbccCommandsInstance
            = new Lazy<IList<DbccCommand>>(() => InitDeprecatedDbccCommandsInstance(), true);

        public DeprecatedDbccCommandRule() : base()
        {
        }

        private static IList<DbccCommand> DeprecatedDbccCommands => DeprecatedDbccCommandsInstance.Value;

        public override void Visit(DbccStatement node)
        {
            if (!DeprecatedDbccCommands.Contains(node.Command))
            {
                return;
            }

            HandleNodeError(node);
        }

        private static IList<DbccCommand> InitDeprecatedDbccCommandsInstance()
        {
            return new List<DbccCommand>
            {
                DbccCommand.DBReindex,
                DbccCommand.DBReindexAll,
                DbccCommand.IndexDefrag,
                DbccCommand.ShowContig,
                DbccCommand.PinTable,
                DbccCommand.UnpinTable,
            };
        }
    }
}
