using NUnit.Framework;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Formatting")]
    [TestOfRule(typeof(SingleSpaceAfterSingleLineCommentBeginRule))]
    public class SingleSpaceAfterSingleLineCommentBeginRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        protected override void DoAfterRuleInstantiated(AbstractRule rule)
        {
            if (!(rule is ICommentAnalyzer ca))
            {
                throw new InvalidOperationException("Bad rule class");
            }

            ca.LoadSpecialCommentPrefixes(new List<string> { "Sql Prompt" });
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(SingleSpaceAfterSingleLineCommentBeginRuleTests));
        }
    }
}
