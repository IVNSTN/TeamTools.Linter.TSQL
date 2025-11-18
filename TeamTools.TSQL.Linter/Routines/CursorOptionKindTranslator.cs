using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Routines
{
    public static class CursorOptionKindTranslator
    {
        private static readonly Lazy<Dictionary<CursorOptionKind, string>> OptionNamesInstance
            = new Lazy<Dictionary<CursorOptionKind, string>>(() => InitOptionNamesInstance(), true);

        private static Dictionary<CursorOptionKind, string> OptionNames => OptionNamesInstance.Value;

        public static string GetName(CursorOptionKind opt) => OptionNames.GetValueOrDefault(opt, opt.ToString());

        private static Dictionary<CursorOptionKind, string> InitOptionNamesInstance()
        {
            return new Dictionary<CursorOptionKind, string>
            {
                { CursorOptionKind.Dynamic, "DYNAMIC" },
                { CursorOptionKind.FastForward, "FAST_FORWARD" },
                { CursorOptionKind.ForwardOnly, "FORWARD_ONLY" },
                { CursorOptionKind.Global, "GLOBAL" },
                { CursorOptionKind.Insensitive, "INSENSITIVE" },
                { CursorOptionKind.Keyset, "KEYSET" },
                { CursorOptionKind.Local, "LOCAL" },
                { CursorOptionKind.Optimistic, "OPTIMISTIC" },
                { CursorOptionKind.ReadOnly, "READ_ONLY" },
                { CursorOptionKind.Scroll, "SCROLL" },
                { CursorOptionKind.ScrollLocks, "SCROLL_LOCKS" },
                { CursorOptionKind.Static, "STATIC" },
                { CursorOptionKind.TypeWarning, "TYPE_WARNING" },
            };
        }
    }
}
