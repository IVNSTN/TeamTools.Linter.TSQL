using System;
using System.Collections.Generic;
using System.Linq;

namespace TeamTools.TSQL.Linter.Routines
{
    internal class SystemProcDetector
    {
        private static readonly ICollection<string> SysProcPrefixes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "sp_",
            "xp_",
            "sys_",
            "sysmail_",
        };

        public bool IsSystemProc(string identifier)
        {
            if (identifier is null)
            {
                return false;
            }

            return SysProcPrefixes.Any(pref => identifier.StartsWith(pref, StringComparison.OrdinalIgnoreCase));
        }
    }
}
