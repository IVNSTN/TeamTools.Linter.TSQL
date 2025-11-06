using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DE0405", "DEPRECATED_OPTIONS")]
    internal sealed class DeprecatedOptionRule : AbstractRule
    {
        // on = true, off = false, null = any state
        private static readonly Lazy<IDictionary<string, bool?>> DeprecatedOptionsInstance
            = new Lazy<IDictionary<string, bool?>>(() => InitDeprecatedOptionsInstance(), true);

        public DeprecatedOptionRule() : base()
        {
        }

        private static IDictionary<string, bool?> DeprecatedOptions => DeprecatedOptionsInstance.Value;

        public override void Visit(TSqlBatch node)
        {
            var optVisitor = new SetOptionsVisitor();
            node.AcceptChildren(optVisitor);

            foreach (var opt in optVisitor.DetectedOptions.Keys)
            {
                if (DeprecatedOptions.ContainsKey(opt) && (null == DeprecatedOptions[opt]
                    || optVisitor.DetectedOptions[opt].Contains(DeprecatedOptions[opt] ?? false)))
                {
                    // TODO : point to specific line
                    HandleNodeError(node);
                }
            }
        }

        private static string ToStr(SetOptions opt) => opt.ToString().ToUpperInvariant();

        private static IDictionary<string, bool?> InitDeprecatedOptionsInstance()
        {
            return new SortedDictionary<string, bool?>(StringComparer.OrdinalIgnoreCase)
            {
                { ToStr(SetOptions.FmtOnly), null },
                { ToStr(SetOptions.ForcePlan), null },
                { ToStr(SetOptions.RemoteProcTransactions), null },
                { ToStr(SetOptions.AnsiNulls), false },
                { ToStr(SetOptions.ConcatNullYieldsNull), false },
                { ToStr(SetOptions.AnsiPadding), false },
                { ToStr(SetOptions.AnsiNullDfltOff), null },
                { ToStr(SetOptions.AnsiNullDfltOn), null },
                { ToStr(SetOptions.AnsiDefaults), null },
                { ToStr(SetOptions.ShowPlanText), null },
                { ToStr(SetOptions.ShowPlanXml), null },
                { ToStr(SetOptions.ShowPlanAll), null },
            };
        }
    }
}
