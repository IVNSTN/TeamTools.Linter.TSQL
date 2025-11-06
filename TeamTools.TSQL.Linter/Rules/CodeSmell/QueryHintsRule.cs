using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0135", "BAD_QUERY_HINT")]
    internal sealed class QueryHintsRule : AbstractRule
    {
        private static readonly Lazy<IDictionary<string, string>> ForbiddenHintsInstance
            = new Lazy<IDictionary<string, string>>(() => InitForbiddenHintsInstance(), true);

        public QueryHintsRule() : base()
        {
        }

        private static IDictionary<string, string> ForbiddenHints => ForbiddenHintsInstance.Value;

        public override void Visit(TableHint node)
        {
            string key = "tab-" + node.HintKind.ToString();
            if (ForbiddenHints.ContainsKey(key))
            {
                HandleNodeError(node, ForbiddenHints[key]);
            }
        }

        public override void Visit(OptimizerHint node)
        {
            string key = "qry-" + node.HintKind.ToString();
            if (ForbiddenHints.ContainsKey(key))
            {
                HandleNodeError(node, ForbiddenHints[key]);
            }
        }

        public override void Visit(UseHintList node)
        {
            HandleNodeError(node, "USE HINT");
        }

        private static IDictionary<string, string> InitForbiddenHintsInstance()
        {
            return new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "tab-" + TableHintKind.NoLock.ToString(), "NOLOCK" },
                { "tab-" + TableHintKind.IgnoreTriggers.ToString(), "IGNORE_TRIGGERS" },
                { "tab-" + TableHintKind.FastFirstRow.ToString(), "FASTFIRSTROW" },
                { "qry-" + OptimizerHintKind.Fast.ToString(), "FAST <N>" },
            };
        }
    }
}
