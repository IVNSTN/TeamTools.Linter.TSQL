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
        private static readonly Lazy<List<Tuple<CursorOptionKind, CursorOptionKind>>> IncompatibleOptionsInstance
            = new Lazy<List<Tuple<CursorOptionKind, CursorOptionKind>>>(() => InitIncompatibleOptionsInstance(), true);

        private static readonly HashSet<CursorOptionKind> ReadOnlyFlags = new HashSet<CursorOptionKind> { CursorOptionKind.FastForward, CursorOptionKind.ReadOnly };

        public CursorOptionsCompatibilityRule() : base()
        {
        }

        private static List<Tuple<CursorOptionKind, CursorOptionKind>> IncompatibleOptions => IncompatibleOptionsInstance.Value;

        public override void Visit(CursorDefinition node)
        {
            if (node.Options.Count == 0)
            {
                return;
            }

            ValidateReadOnlyOptions(node, ViolationHandlerWithMessage);

            if (node.Options.Count < 2)
            {
                return;
            }

            ValidateOptionCombination(node, ViolationHandlerWithMessage);
        }

        private static List<Tuple<CursorOptionKind, CursorOptionKind>> InitIncompatibleOptionsInstance()
        {
            return new List<Tuple<CursorOptionKind, CursorOptionKind>>
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

        private static Tuple<CursorOptionKind, CursorOptionKind> MakeOptionPair(CursorOptionKind a, CursorOptionKind b)
        {
            return new Tuple<CursorOptionKind, CursorOptionKind>(a, b);
        }

        private static string OptName(CursorOption opt) => CursorOptionKindTranslator.GetName(opt.OptionKind);

        private static void ValidateReadOnlyOptions(CursorDefinition node, Action<TSqlFragment, string> callback)
        {
            // cannot be ro if for update defined
            if (node.Select?.QueryExpression is QuerySpecification q
            && q.ForClause is UpdateForClause upd)
            {
                int n = node.Options.Count;
                for (int i = 0; i < n; i++)
                {
                    var opt = node.Options[i];
                    if (ReadOnlyFlags.Contains(opt.OptionKind))
                    {
                        callback(upd, string.Format(ErrorDetailsTemplate, "FOR UPDATE OF", OptName(opt)));
                        return;
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
                where o1 != o2 && i.Item1 == o1.OptionKind && i.Item2 == o2.OptionKind
                select new Tuple<CursorOption, CursorOption>(o1, o2);

            // searching for illegal option combination
            foreach (var badOpt in badOptionsCombination)
            {
                callback(badOpt.Item2, string.Format(ErrorDetailsTemplate, OptName(badOpt.Item1), OptName(badOpt.Item2)));
            }
        }
    }
}
