using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Files")]
    [TestOfRule(typeof(CrlfRule))]
    public sealed class CrlfRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            // If we are on Unix, file sources will not have CRLF after checkout from GIT
            Assume.That(Environment.NewLine == "\r\n", "Skipping on Unix");

            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        [Test]
        public void TestCrlfRuleFailsIfLf()
        {
            int errCnt = 0;
            var rule = new CrlfRule();
            rule.ViolationCallback += (obj, dto) => errCnt++;

            rule.Validate(MockLinter.MakeLinter().Lint(
            @"
                select 1
                select 2
            "
            .Replace("\r", ""), out IList<ParseError> err));

            Assert.That(err, Is.Empty, "failed parsing LF");
            Assert.That(errCnt, Is.EqualTo(3), "LF only");
        }

        [Test]
        public void TestCrlfRuleFailsIfMixedLineEnding()
        {
            int errCnt = 0;
            var rule = new CrlfRule();
            rule.ViolationCallback += (obj, dto) => errCnt++;

            rule.Validate(MockLinter.MakeLinter().Lint(
            @"select 1
                select 2
            "
            .Replace("1\r\n", "1\r")
            .Replace("2\r\n", "2\n"), out IList<ParseError> err));

            Assert.That(err, Is.Empty, "failed parsing mixed");
            Assert.That(errCnt, Is.EqualTo(2), "mixed");
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(CrlfRuleTests));
        }
    }
}
