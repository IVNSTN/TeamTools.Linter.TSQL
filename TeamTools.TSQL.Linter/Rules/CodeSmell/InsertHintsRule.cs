using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0917", "FORBIDDEN_INSERT_HINTS")]
    // TODO : Split rule into forbidden and deprecated hints detectors
    internal sealed class InsertHintsRule : AbstractRule
    {
        private static readonly Lazy<Dictionary<TableHintKind, string>> ForbiddenHintsInstance
            = new Lazy<Dictionary<TableHintKind, string>>(() => InitForbiddenHintsInstance(), true);

        public InsertHintsRule() : base()
        {
        }

        private static Dictionary<TableHintKind, string> ForbiddenHints => ForbiddenHintsInstance.Value;

        public override void Visit(InsertStatement node)
        {
            var hints = node.InsertSpecification.Target is NamedTableReference tbl ? tbl.TableHints : null;
            if (hints is null || hints.Count == 0)
            {
                return;
            }

            int n = hints.Count;
            for (int i = 0; i < n; i++)
            {
                var hint = hints[i];
                if (ForbiddenHints.TryGetValue(hint.HintKind, out var hintName))
                {
                    HandleNodeError(hint, hintName);
                }
            }
        }

        private static Dictionary<TableHintKind, string> InitForbiddenHintsInstance()
        {
            return new Dictionary<TableHintKind, string>
            {
                { TableHintKind.ForceSeek, "FORCESEEK" },
                { TableHintKind.ForceScan, "FORCESCAN" },
                { TableHintKind.IgnoreTriggers, "IGNORE_TRIGGERS" },
                { TableHintKind.IgnoreConstraints, "IGNORE_CONSTRAINTS" },
                { TableHintKind.ReadPast, "READPAST" },
                { TableHintKind.ReadCommittedLock, "READCOMMITTEDLOCK" },
                { TableHintKind.Rowlock, "ROWLOCK" },
                { TableHintKind.PagLock, "PAGLOCK" },
                { TableHintKind.TabLock, "TABLOCK" },  // TODO : Actually it can unlock parallel insert. Should be allowed at least for temp tables
                { TableHintKind.NoLock, "NOLOCK" },

                // below are deprecated lock hints for insert targets
                { TableHintKind.HoldLock, "HOLDLOCK" },
                { TableHintKind.Serializable, "SERIALIZABLE" },
                { TableHintKind.ReadCommitted, "READCOMMITTED" },
                { TableHintKind.RepeatableRead, "REPEATABLEREAD" },
                { TableHintKind.UpdLock, "UPDLOCK" },
            };
        }
    }
}
