using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0405", "DEPRECATED_OPTIONS")]
    internal sealed class DeprecatedOptionRule : AbstractRule
    {
        // on = true, off = false, null = any state is deprecated
        private static readonly Dictionary<SetOptions, bool?> DeprecatedOptions = new Dictionary<SetOptions, bool?>
        {
            { SetOptions.FmtOnly, null },
            { SetOptions.ForcePlan, null },
            { SetOptions.RemoteProcTransactions, null },
            { SetOptions.AnsiNulls, false },
            { SetOptions.ConcatNullYieldsNull, false },
            { SetOptions.AnsiPadding, false },
            { SetOptions.AnsiNullDfltOff, null },
            { SetOptions.AnsiNullDfltOn, null },
            { SetOptions.AnsiDefaults, null },
            { SetOptions.ShowPlanText, null },
            { SetOptions.ShowPlanXml, null },
            { SetOptions.ShowPlanAll, null },
        };

        private static readonly Dictionary<SetOptions, string> DeprecatedOptionText = new Dictionary<SetOptions, string>
        {
            { SetOptions.FmtOnly, "FMTONLY" },
            { SetOptions.ForcePlan, "FORCE_PLAN" },
            { SetOptions.RemoteProcTransactions, "REMOTE_PROC_TRANSACTIONS" },
            { SetOptions.AnsiNulls, "ANSI_NULLS" },
            { SetOptions.ConcatNullYieldsNull, "CONCAT_NULL_YIELDS_NULL" },
            { SetOptions.AnsiPadding, "ANSI_PADDING" },
            { SetOptions.AnsiNullDfltOff, "ANSI_NULL_DFLT_OFF" },
            { SetOptions.AnsiNullDfltOn, "ANSI_NULL_DFLT_ON" },
            { SetOptions.AnsiDefaults, "ANSI_DEFAULTS" },
            { SetOptions.ShowPlanText, "SHOWPLAN_TEXT" },
            { SetOptions.ShowPlanXml, "SHOWPLAN_XML" },
            { SetOptions.ShowPlanAll, "SHOWPLAN_ALL" },
        };

        public DeprecatedOptionRule() : base()
        {
        }

        public override void Visit(PredicateSetStatement node)
        {
            foreach (var opt in DeprecatedOptions)
            {
                if (node.Options.HasFlag(opt.Key)
                && (opt.Value is null || node.IsOn == opt.Value))
                {
                    HandleNodeError(node, DeprecatedOptionText[opt.Key]);
                }
            }
        }
    }
}
