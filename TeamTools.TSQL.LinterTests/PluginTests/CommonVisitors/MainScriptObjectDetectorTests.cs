using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(MainScriptObjectDetector))]
    public sealed class MainScriptObjectDetectorTests
    {
        private MainScriptObjectDetector mainObject;
        private MockLinter linter;
        private IList<ParseError> err;

        [SetUp]
        public void SetUp()
        {
            mainObject = new MainScriptObjectDetector();
            linter = MockLinter.MakeLinter();
            err = null;
        }

        [Test]
        public void TestCreateView()
        {
            mainObject.Analyze(linter.Lint(
            @"
                CREATE VIEW dbo.foo_view AS select 1
                GO
                CREATE INDEX idx_foo_idx on dbo.foo(name)
                GO
            ", out err));

            Assert.That(err, Is.Empty, "failed parsing 1");
            Assert.That(mainObject.ObjectFullName, Is.EqualTo("dbo.foo_view"), "create view");
        }

        [Test]
        public void TestCreateSchema()
        {
            mainObject = new MainScriptObjectDetector();
            mainObject.Analyze(linter.Lint(
            @"
                SET NOCOUNT ON
                GO
                CREATE schema bar
                GO
            ", out err));

            Assert.That(err, Is.Empty, "failed parsing 2");
            Assert.That(mainObject.ObjectFullName, Is.EqualTo("bar"), "create schema");
        }

        [Test]
        public void TestNoCreateScriptGivesEmptyName()
        {
            mainObject.Analyze(linter.Lint(
            @"
                SET NOCOUNT ON
                GO
                DECLARE @foo VARCHAR(3)
                set @foo = 'bar'
            ", out err));

            Assert.That(err, Is.Empty, "failed parsing 3");
            Assert.That(mainObject.ObjectFullName, Is.EqualTo(""), "no create");
        }
    }
}
