using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;
using TeamTools.TSQL.Linter.Rules;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(AbstractRule))]
    public sealed class AbstractRuleTests
    {
        [Test]
        public void TestAbstractRuleErrorHandlers()
        {
            int errCnt = 0;
            int errRow = -1;
            int errCol = -1;
            IList<ParseError> err = new List<ParseError>();
            var rule = new MockRule();

            rule.ViolationCallback += (obj, dto) =>
            {
                errCnt++;
                errRow = dto.Line;
                errCol = dto.Column;
            };

            rule.HandleLineError(32, 23);
            Assert.That(errCnt, Is.EqualTo(1), "line error count");
            Assert.That(errRow, Is.EqualTo(32), "line error row");
            Assert.That(errCol, Is.EqualTo(23), "line error column");

            rule.HandleFileErrorWrap();
            Assert.That(errCnt, Is.EqualTo(2), "file error count");
            Assert.That(errRow, Is.EqualTo(0), "file error row");
            Assert.That(errCol, Is.EqualTo(1), "file error column");
        }

        [Test]
        public void TestAbstractRuleNodeTextExtraction()
        {
            int errCnt = 0;
            int errRow = -1;
            int errCol = -1;
            IList<ParseError> err = new List<ParseError>();
            var rule = new MockRule();

            rule.ViolationCallback += (obj, dto) =>
            {
                errCnt++;
                errRow = dto.Line;
                errCol = dto.Column;
            };

            var linter = MockLinter.MakeLinter();

            linter.Lint("SELECT 1 as abcd", out err).Accept(rule);
            Assert.That(err, Is.Empty, "error parsing 1");

            Assert.That(rule.NodeText, Is.EqualTo("SELECT 1 as abcd"), "node text equals");
        }

        private class MockRule : AbstractRule
        {
            public MockRule() : base()
            {
            }

            public string NodeText { get; private set; }

            public void HandleFileErrorWrap()
            {
                HandleFileError();
            }

            public override void Visit(TSqlFragment node)
            {
                if (!string.IsNullOrEmpty(NodeText))
                {
                    return;
                }

                NodeText = node.GetFragmentText();
            }
        }
    }
}
