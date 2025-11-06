using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Failure")]
    [TestOfRule(typeof(WindowFunctionRequiresOrderByRule))]
    public class WindowFunctionRequiresOrderByRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(WindowFunctionRequiresOrderByRuleTests));
            /* parsing fails, so testing is currently not possible.
                yield return new RuleTestCasePreset(
                    testSourceFile: "rows_range_missing_order_by_raise_2_violations.sql",
                    expectedViolations: 2,
                    script: @"
                    SELECT
                        COUNT(t.distance) OVER(PARTITION BY t.zone ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) as mdist_rows
                        , SUM(t.distance) OVER(PARTITION BY t.zone RANGE UNBOUNDED PRECEDING) as mdist_range
                    ");
                */
        }
    }
}
