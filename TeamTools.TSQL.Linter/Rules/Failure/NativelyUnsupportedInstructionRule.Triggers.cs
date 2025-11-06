using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Trigger specifics.
    /// </summary>
    internal partial class NativelyUnsupportedInstructionRule
    {
        private static readonly ICollection<TriggerOptionKind> UnsupportedTriggerOptions = new List<TriggerOptionKind>
        {
            TriggerOptionKind.Encryption,
        };

        public override void Visit(TriggerStatementBody node)
        {
            if (!node.Options.Any(opt => opt.OptionKind == TriggerOptionKind.NativeCompile))
            {
                return;
            }

            if (node.IsNotForReplication)
            {
                HandleNodeError(node.Options.FirstOrDefault() as TSqlFragment ?? node, "NOT FOR REPLICATION");
            }

            node.Options
                .Where(opt => UnsupportedTriggerOptions.Contains(opt.OptionKind))
                .ToList()
                .ForEach(opt => HandleNodeError(opt, opt.OptionKind.ToString().ToUpperInvariant()));

            DoValidateStatements(node, node.StatementList);
        }
    }
}
