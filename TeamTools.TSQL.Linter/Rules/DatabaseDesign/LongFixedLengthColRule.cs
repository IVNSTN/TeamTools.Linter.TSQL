using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0750", "LONG_FIXED_LENGTH_COL")]
    internal sealed class LongFixedLengthColRule : BaseColumnLengthControlRule
    {
        private static readonly int MaxSizeForFixedType = 12;

        private static readonly ICollection<string> FixedTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dbo.CHAR",
            "dbo.NCHAR",
            "dbo.BINARY",
        };

        public LongFixedLengthColRule() : base(0, MaxSizeForFixedType, FixedTypes)
        {
        }
    }
}
