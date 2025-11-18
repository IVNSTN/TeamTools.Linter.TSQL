using System;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.Linter.Rules
{
    [RuleIdentity("DD0750", "LONG_FIXED_LENGTH_COL")]
    internal sealed class LongFixedLengthColRule : BaseColumnLengthControlRule
    {
        private static readonly int MaxSizeForFixedType = 12;

        private static readonly HashSet<string> FixedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            TSqlDomainAttributes.Types.Char,
            TSqlDomainAttributes.Types.NChar,
            TSqlDomainAttributes.Types.Binary,
        };

        public LongFixedLengthColRule() : base(0, MaxSizeForFixedType, FixedTypes)
        {
        }
    }
}
