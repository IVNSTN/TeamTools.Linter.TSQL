using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("FA0902", "CURSOR_OPTION_INCOMPATIBLE")]
    internal sealed class CursorOptionsCompatibilityRule : AbstractRule
    {
        private static readonly string ErrorDetailsTemplate = "{1} vs {0}";
        private static readonly Lazy<ICollection<KeyValuePair<CursorOptionKind, CursorOptionKind>>> IncompatibleOptionsInstance
            = new Lazy<ICollection<KeyValuePair<CursorOptionKind, CursorOptionKind>>>(() => InitIncompatibleOptionsInstance(), true);

        private static readonly ICollection<CursorOptionKind> ReadOnlyFlags = new CursorOptionKind[] { CursorOptionKind.FastForward, CursorOptionKind.ReadOnly };

        public CursorOptionsCompatibilityRule() : base()
        {
        }

        private static ICollection<KeyValuePair<CursorOptionKind, CursorOptionKind>> IncompatibleOptions => IncompatibleOptionsInstance.Value;

        public override void Visit(CursorDefinition node)
        {
            if (node.Options.Count == 0)
            {
                return;
            }

            ValidateReadOnlyOptions(node, HandleNodeError);

            if (node.Options.Count < 2)
            {
                return;
            }

            ValidateOptionCombination(node, HandleNodeError);
        }

        private static ICollection<KeyValuePair<CursorOptionKind, CursorOptionKind>> InitIncompatibleOptionsInstance()
        {
            return new List<KeyValuePair<CursorOptionKind, CursorOptionKind>>
            {
                MakeOptionPair(CursorOptionKind.Local, CursorOptionKind.Global),
                MakeOptionPair(CursorOptionKind.Scroll, CursorOptionKind.ForwardOnly),
                MakeOptionPair(CursorOptionKind.Scroll, CursorOptionKind.FastForward),
                MakeOptionPair(CursorOptionKind.Static, CursorOptionKind.Keyset),
                MakeOptionPair(CursorOptionKind.Static, CursorOptionKind.Dynamic),
                MakeOptionPair(CursorOptionKind.Static, CursorOptionKind.FastForward),
                MakeOptionPair(CursorOptionKind.Static, CursorOptionKind.ScrollLocks),
                MakeOptionPair(CursorOptionKind.Dynamic, CursorOptionKind.Keyset),
                MakeOptionPair(CursorOptionKind.FastForward, CursorOptionKind.Dynamic),
                MakeOptionPair(CursorOptionKind.FastForward, CursorOptionKind.Keyset),
                MakeOptionPair(CursorOptionKind.FastForward, CursorOptionKind.ScrollLocks),
                MakeOptionPair(CursorOptionKind.FastForward, CursorOptionKind.Optimistic),
                MakeOptionPair(CursorOptionKind.FastForward, CursorOptionKind.ForwardOnly),
                MakeOptionPair(CursorOptionKind.FastForward, CursorOptionKind.ReadOnly),
            };
        }

        private static KeyValuePair<CursorOptionKind, CursorOptionKind> MakeOptionPair(CursorOptionKind a, CursorOptionKind b)
        {
            return new KeyValuePair<CursorOptionKind, CursorOptionKind>(a, b);
        }

        private static string OptName(CursorOption opt) => CursorOptionKindTranslator.GetName(opt.OptionKind);

        private static void ValidateReadOnlyOptions(CursorDefinition node, Action<TSqlFragment, string> callback)
        {
            // cannot be ro if for update defined
            if (node.Select?.QueryExpression is QuerySpecification q)
            {
                if (q.ForClause is UpdateForClause upd)
                {
                    var readonlyOption = node.Options.FirstOrDefault(opt => ReadOnlyFlags.Contains(opt.OptionKind));

                    if (readonlyOption != null)
                    {
                        callback(q.ForClause, string.Format(ErrorDetailsTemplate, "FOR UPDATE OF", OptName(readonlyOption)));
                    }
                }
            }
        }

        private static void ValidateOptionCombination(CursorDefinition node, Action<TSqlFragment, string> callback)
        {
            var badOptionsCombination =
                from o1 in node.Options
                from o2 in node.Options
                from i in IncompatibleOptions
                where o1 != o2 && i.Key == o1.OptionKind && i.Value == o2.OptionKind
                select new KeyValuePair<CursorOption, CursorOption>(o1, o2);

            // searching for illegal option combination
            foreach (var badOpt in badOptionsCombination)
            {
                callback(badOpt.Value, string.Format(ErrorDetailsTemplate, OptName(badOpt.Key), OptName(badOpt.Value)));
            }
        }
    }
}
