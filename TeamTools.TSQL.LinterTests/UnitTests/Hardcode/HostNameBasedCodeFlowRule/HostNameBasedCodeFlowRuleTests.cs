using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Hardcode")]
    [TestOfRule(typeof(HostNameBasedCodeFlowRule))]
    public class HostNameBasedCodeFlowRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(HostNameBasedCodeFlowRuleTests));
        }
    }
}
