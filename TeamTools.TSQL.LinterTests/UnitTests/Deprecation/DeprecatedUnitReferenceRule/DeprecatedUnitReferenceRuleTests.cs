using NUnit.Framework;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Deprecation")]
    [TestOfRule(typeof(DeprecatedUnitReferenceRule))]
    public sealed class DeprecatedUnitReferenceRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        protected override void DoAfterRuleInstantiated(AbstractRule rule)
        {
            if (!(rule is IDeprecationHandler dp))
            {
                throw new InvalidOperationException("Bad rule class");
            }

            dp.LoadDeprecations(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "dbo.f_today", "use getdate" },
                { "dbo.bad_table", "use good tables" },
            });
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(DeprecatedUnitReferenceRuleTests));
        }
    }
}
