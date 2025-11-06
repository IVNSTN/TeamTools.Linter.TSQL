using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Files")]
    [TestOfRule(typeof(AllowedEncodingRule))]
    public sealed class AllowedEncodingRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        [Test]
        public void TestRuleFailsOnMissingFile()
        {
            try
            {
                var rule = new AllowedEncodingRule();
                rule.ViolationCallback += (obj, dto) => Assert.Fail("unexpected rule violation");

                rule.VerifyFile("missing file.txt");

                Assert.Fail("no error on missing file");
            }
            catch (FileNotFoundException)
            {
                Assert.Pass();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(AllowedEncodingRuleTests));
        }
    }
}
