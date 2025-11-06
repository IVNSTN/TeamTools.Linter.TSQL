using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("CS0917", "FORBIDDEN_INSERT_HINTS")]
    internal sealed class InsertHintsRule : AbstractRule
    {
        private static readonly Lazy<ICollection<string>> ForbiddenHintsInstance
            = new Lazy<ICollection<string>>(() => InitForbiddenHintsInstance(), true);

        public InsertHintsRule() : base()
        {
        }

        private static ICollection<string> ForbiddenHints => ForbiddenHintsInstance.Value;

        public override void Visit(InsertStatement node)
        {
            var hints = node.InsertSpecification.Target is NamedTableReference tbl ? tbl.TableHints : null;
            if (hints == null || hints.Count == 0)
            {
                return;
            }

            var illegalHints = hints
                .Select(h => h.HintKind.ToString())
                .Where(h => ForbiddenHints.Contains(h))
                .OrderBy(h => h);

            if (!illegalHints.Any())
            {
                return;
            }

            HandleNodeError(node, string.Join(", ", illegalHints));
        }

        private static ICollection<string> InitForbiddenHintsInstance()
        {
            return new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "FORCESEEK",
                "FORCESCAN",
                "IGNORE_TRIGGERS",
                "IGNORE_CONSTRAINTS",
                "READPAST",
                "READCOMMITTEDLOCK",
                "ROWLOCK",
                "PAGLOCK",
                "TABLOCK",
                // below are deprecated lock hints for insert targets
                "HOLDLOCK",
                "SERIALIZABLE",
                "READCOMMITTED",
                "REPEATABLEREAD",
                "UPDLOCK",
            };
        }
    }
}
