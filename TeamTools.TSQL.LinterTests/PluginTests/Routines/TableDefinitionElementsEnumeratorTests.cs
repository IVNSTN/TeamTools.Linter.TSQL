using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System.IO;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(TableDefinitionElementsEnumerator))]
    public sealed class TableDefinitionElementsEnumeratorTests
    {
        private const string SqlScript = @"
CREATE TABLE dbo.foo (
    id   INT NOT NULL,
    sps  INT SPARSE NULL,
    calc AS id + 1 PERSISTED
);";

        private MockLinter linter;

        [OneTimeSetUp]
        public void SetUp()
        {
            linter = MockLinter.MakeDefaultLinter();
        }

        [Test]
        public void ItRespectsComputedCols()
        {
            var tblInfo = Parse(SqlScript);

            Assert.That(tblInfo, Is.Not.Null);
            Assert.That(tblInfo.Tables.Count, Is.EqualTo(1));
            Assert.That(tblInfo.Tables["dbo.foo"].Columns.Count, Is.EqualTo(3));
            Assert.That(tblInfo.Tables["dbo.foo"].Columns["calc"].IsComputed, Is.True);
        }

        [Test]
        public void ItDetectsSparseCols()
        {
            var tblInfo = Parse(SqlScript);

            Assert.That(tblInfo, Is.Not.Null);
            Assert.That(tblInfo.Tables.Count, Is.EqualTo(1));
            Assert.That(tblInfo.Tables["dbo.foo"].Columns.Count, Is.EqualTo(3));
            Assert.That(tblInfo.Tables["dbo.foo"].Columns["sps"].IsSparse, Is.True);
        }

        [Test]
        public void ItDetectsNullabilityAttr()
        {
            var tblInfo = Parse(SqlScript);

            Assert.That(tblInfo, Is.Not.Null);
            Assert.That(tblInfo.Tables.Count, Is.EqualTo(1));
            Assert.That(tblInfo.Tables["dbo.foo"].Columns.Count, Is.EqualTo(3));
            Assert.That(tblInfo.Tables["dbo.foo"].Columns["sps"].IsNullable, Is.True);
            Assert.That(tblInfo.Tables["dbo.foo"].Columns["id"].IsNullable, Is.False);
        }

        private TableDefinitionElementsEnumerator Parse(string sql)
        {
            using var reader = new StringReader(sql);
            return new TableDefinitionElementsEnumerator(linter.Parser.Parse(reader, out _) as TSqlScript);
        }
    }
}
