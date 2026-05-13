using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.Collections.Generic;

namespace TeamTools.TSQL.Linter.Rules
{
    internal partial class TempTableCachingRule
    {
        private void DetectBadProcOption(IList<ProcedureOption> options)
        {
            for (int i = options.Count - 1; i >= 0; i--)
            {
                var opt = options[i];
                if (opt.OptionKind == ProcedureOptionKind.Recompile)
                {
                    HandleNodeError(opt, "RECOMPILE");
                    return;
                }
            }
        }
    }
}
