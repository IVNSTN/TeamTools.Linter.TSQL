using NUnit.Framework;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Redundancy")]
    [TestOfRule(typeof(RedundantSquareBracketsRule))]
    public sealed class RedundantSquareBracketsRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        protected override void DoAfterRuleInstantiated(AbstractRule rule)
        {
            if (!(rule is IKeywordDetector kw))
            {
                throw new InvalidOperationException("Bad rule class");
            }

            kw.LoadKeywords(new List<string>() { "INT", "RETURN", "ORDER" });
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(RedundantSquareBracketsRuleTests));
        }
    }
}
