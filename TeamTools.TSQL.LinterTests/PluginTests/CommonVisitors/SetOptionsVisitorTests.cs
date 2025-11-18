using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(SetOptionsVisitor))]
    public sealed class SetOptionsVisitorTests
    {
        private SetOptionsVisitor visitor;
        private MockLinter linter;
        private IList<ParseError> err;

        [SetUp]
        public void SetUp()
        {
            visitor = new SetOptionsVisitor();
            linter = MockLinter.MakeLinter();
            err = null;
        }

        [Test]
        public void TestOptionSwitchesDetected()
        {
            linter.Lint("SET NOCOUNT ON", out err).Accept(visitor);
            Assert.That(err, Is.Empty, "failed parsing 1");
            Assert.That(visitor.DetectedOptions, Has.Count.EqualTo(1));
            Assert.That(visitor.DetectedOptions, Does.ContainKey(SetOptions.NoCount));
            Assert.That(visitor.DetectedOptions[SetOptions.NoCount], Is.True);

            linter.Lint("SET XACT_ABORT OFF", out err).Accept(visitor);
            Assert.That(err, Is.Empty, "failed parsing 2");
            Assert.That(visitor.DetectedOptions, Does.ContainKey(SetOptions.XactAbort));
            Assert.That(visitor.DetectedOptions[SetOptions.XactAbort], Is.False);
        }
    }
}
