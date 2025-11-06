using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
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
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        [Test]
        public void TestCrlfPatternMatchesLf()
        {
            var rule = new CrlfRule();

            Assert.Multiple(
            () =>
            {
                Assert.That(rule.Pattern.IsMatch("adsf"), Is.False, "one line");
                Assert.That(rule.Pattern.IsMatch("adsf" + (char)10 + "asdf"), Is.True, "lf");
                Assert.That(rule.Pattern.IsMatch("adsf" + (char)13 + (char)10 + "asdf"), Is.False, "crlf");
            });
        }

        [Test]
        public void TestCrlfRuleFailsIfLf()
        {
            int errCnt = 0;
            var rule = new CrlfRule();
            rule.ViolationCallback += (obj, dto) => errCnt++;

            MockLinter.MakeLinter().Lint(
            @"
                select 1
                select 2
            "
            .Replace("\r", ""), out IList<ParseError> err)
            .Accept(rule);

            Assert.That(err, Is.Empty, "failed parsing LF");
            Assert.That(errCnt, Is.EqualTo(3), "LF only");
        }

        [Test]
        public void TestCrlfRuleFailsIfMixedLineEnding()
        {
            int errCnt = 0;
            var rule = new CrlfRule();
            rule.ViolationCallback += (obj, dto) => errCnt++;

            MockLinter.MakeLinter().Lint(
            @"select 1
                select 2
            "
            .Replace("1\r\n", "1\r")
            .Replace("2\r\n", "2\n"), out IList<ParseError> err)
            .Accept(rule);

            Assert.That(err, Is.Empty, "failed parsing mixed");
            Assert.That(errCnt, Is.EqualTo(2), "mixed");
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(CrlfRuleTests));
        }
    }
}
