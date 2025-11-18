using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TeamTools.TSQL.Linter.Infrastructure;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(TSqlParserFactory))]
    public sealed class TSqlParserFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void TestTSqlParserFactoryMakesRequestedversion()
        {
            var p = MakeParserFor(110);
            Assert.That(p, Is.InstanceOf<TSql110Parser>());

            p = MakeParserFor(130);
            Assert.That(p, Is.InstanceOf<TSql130Parser>());

            p = MakeParserFor(140);
            Assert.That(p, Is.InstanceOf<TSql140Parser>());

            p = MakeParserFor(150);
            Assert.That(p, Is.InstanceOf<TSql150Parser>());
        }

        [Test]
        public void TestTSqlParserFactoryFailsOnBadCompatLevel()
        {
            try
            {
                MakeParserFor(333);
                Assert.Fail("accepted dummy compatibility level");
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }
        }

        [Test]
        public void TestTSql150LevelSupportsCorrespondingSyntax()
        {
            const string sql = @"
            declare
            @dt_json        NVARCHAR(MAX)

            SET @dt_json =
            (
                SELECT
                    'test' AS command
                    , (SELECT TOP(1) acc_code, sys_name FROM dbo.acc_access WHERE id > 1 ORDER BY id FOR JSON PATH) AS packet
                FOR JSON PATH);
            ";

            var linter = MakeParserFor(150);
            linter.Parse(new StringReader(sql), out IList<ParseError> err);

            Assert.That(err, Is.Empty);
        }

        private static TSqlParser MakeParserFor(int compatibilityLevel) => TSqlParserFactory.Make(compatibilityLevel);
    }
}
