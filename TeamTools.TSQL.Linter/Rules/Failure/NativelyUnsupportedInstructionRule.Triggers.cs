using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    /// <summary>
    /// Trigger specifics.
    /// </summary>
    internal partial class NativelyUnsupportedInstructionRule
    {
        private static readonly Dictionary<TriggerOptionKind, string> UnsupportedTriggerOptions = new Dictionary<TriggerOptionKind, string>
        {
            { TriggerOptionKind.Encryption, "ENCRYPTION" },
        };

        private void DoValidate(TriggerStatementBody node)
        {
            if (!node.Options.HasOption(TriggerOptionKind.NativeCompile))
            {
                return;
            }

            if (node.IsNotForReplication)
            {
                HandleNodeError(node.Options[0], "NOT FOR REPLICATION");
            }

            int n = node.Options.Count;
            for (int i = 0; i < n; i++)
            {
                var opt = node.Options[i];
                if (UnsupportedTriggerOptions.TryGetValue(opt.OptionKind, out var optionName))
                {
                    HandleNodeError(opt, optionName);
                }
            }

            DoValidateStatements(node, node.StatementList);
        }
    }
}
