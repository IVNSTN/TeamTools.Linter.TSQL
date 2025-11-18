using NUnit.Framework;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Deprecation")]
    [TestOfRule(typeof(DeprecatedTypeRule))]
    public sealed class DeprecatedTypeRuleTests : BaseRuleTest
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
                { "TIMESTAMP", "use ROWVERSION" },
                { "dbo.MyType", "use OtherType" },
            });
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(DeprecatedTypeRuleTests));
        }
    }
}
