using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0749", "SHORT_VAR_LENGTH_COL")]
    internal sealed class ShortVariableLengthColumnRule : BaseColumnLengthControlRule
    {
        private static readonly int MinSizeForVarType = 6;

        private static readonly HashSet<string> VarTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            TSqlDomainAttributes.Types.Varchar,
            TSqlDomainAttributes.Types.NVarchar,
            TSqlDomainAttributes.Types.VarBinary,
        };

        public ShortVariableLengthColumnRule() : base(MinSizeForVarType, 0, VarTypes)
        {
        }
    }
}
