using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.DatabaseDesign")]
    [TestOfRule(typeof(NonClusteredIndexIncludingClusteredColsRule))]
    public class NonClusteredIndexIncludingClusteredColsRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(NonClusteredIndexIncludingClusteredColsRuleTests));
        }
    }
}
