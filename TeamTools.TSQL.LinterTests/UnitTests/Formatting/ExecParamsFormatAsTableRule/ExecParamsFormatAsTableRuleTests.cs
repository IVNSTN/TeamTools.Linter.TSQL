using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Formatting")]
    [TestOfRule(typeof(ExecParamsFormatAsTableRule))]
    public class ExecParamsFormatAsTableRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        [TestCaseSource(nameof(TestCaseAlternativePresets))]
        public void TestWithTrailingComma(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount, new ExecParamsFormatAsTableRule(false));
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(ExecParamsFormatAsTableRuleTests));
        }

        private static IEnumerable<object> TestCaseAlternativePresets()
        {
            return GetTestSources(typeof(ExecParamsFormatAsTableRuleTests), "TestSourcesAlt");
        }
    }
}
