using System;

namespace TeamTools.TSQL.Linter.Routines
{
    internal static class SystemProcDetector
    {
        private static readonly string[] SysProcPrefixes = new string[]
        {
            "sp_",
            "sys_",
            "sysmail_",
            "xp_",
        };

        public static bool IsSystemProc(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return false;
            }

            foreach (var prefix in SysProcPrefixes)
            {
                if (identifier.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
