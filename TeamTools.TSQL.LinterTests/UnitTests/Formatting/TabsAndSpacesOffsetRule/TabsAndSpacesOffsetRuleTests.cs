using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Formatting")]
    [TestOfRule(typeof(TabsAndSpacesOffsetRule))]
    public sealed class TabsAndSpacesOffsetRuleTests : BaseRuleTest
    {
        [TestCaseSource(nameof(TestCasePresets))]
        public override void TestRule(string scriptPath, int expectedViolationCount)
        {
            CheckRuleViolations(scriptPath, expectedViolationCount);
        }

        [Test]
        public void TestTabsAndSpacesOffsetRuleFailsOnMissingFile()
        {
            var rule = new TabsAndSpacesOffsetRule();
            Assert.Throws<FileNotFoundException>(() => rule.VerifyFile("missing file"), "file load fails");
        }

        [Test]
        public void TestTabsAndSpacesOffsetRuleDoesTheSameAsVisitor()
        {
            var linter = MockLinter.MakeDefaultLinter();
            var rule = new MockTabsAndSpacesOffsetRule();
            var src = new StringReader("SELECT 1 AS ID");

            Assert.Throws<InvalidProgramException>(() => rule.Validate(linter.Parser.Parse(src, out IList<ParseError> _)));
        }

        private static IEnumerable<object> TestCasePresets()
        {
            return GetTestSources(typeof(TabsAndSpacesOffsetRuleTests));
        }

        private sealed class MockTabsAndSpacesOffsetRule : TabsAndSpacesOffsetRule
        {
            public MockTabsAndSpacesOffsetRule() : base()
            {
            }

            protected override void CheckTabsAndSpaces(TextReader reader, int startLine, ref OffsetKind offset)
            {
                throw new InvalidProgramException("");
            }
        }
    }
}
