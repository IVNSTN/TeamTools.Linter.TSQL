using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0749", "SHORT_VAR_LENGTH_COL")]
    internal sealed class ShortVariableLengthColumnRule : BaseColumnLengthControlRule
    {
        private static readonly int MinSizeForVarType = 6;

        private static readonly ICollection<string> VarTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dbo.VARCHAR",
            "dbo.NVARCHAR",
            "dbo.VARBINARY",
        };

        public ShortVariableLengthColumnRule() : base(MinSizeForVarType, 0, VarTypes)
        {
        }
    }
}
