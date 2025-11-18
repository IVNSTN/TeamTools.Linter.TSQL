using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0135", "BAD_QUERY_HINT")]
    internal sealed class QueryHintsRule : AbstractRule
    {
        private static readonly Dictionary<TableHintKind, string> ForbiddenTableHints = new Dictionary<TableHintKind, string>
        {
            { TableHintKind.NoLock, "NOLOCK" },
            { TableHintKind.IgnoreTriggers, "IGNORE_TRIGGERS" },
            { TableHintKind.FastFirstRow, "FASTFIRSTROW" },
        };

        private static readonly Dictionary<OptimizerHintKind, string> ForbiddenQueryHints = new Dictionary<OptimizerHintKind, string>
        {
            { OptimizerHintKind.Fast, "FAST <N>" },
        };

        public QueryHintsRule() : base()
        {
        }

        public override void Visit(TableHint node)
        {
            if (ForbiddenTableHints.TryGetValue(node.HintKind, out string badHint))
            {
                HandleNodeError(node, badHint);
            }
        }

        public override void Visit(OptimizerHint node)
        {
            if (ForbiddenQueryHints.TryGetValue(node.HintKind, out string badHint))
            {
                HandleNodeError(node, badHint);
            }
        }

        public override void Visit(UseHintList node)
        {
            HandleNodeError(node, "USE HINT");
        }
    }
}
