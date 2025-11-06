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
        private TSqlParserFactory factory;

        [SetUp]
        public void SetUp()
        {
            factory = new TSqlParserFactory();
        }

        [Test]
        public void TestTSqlParserFactoryMakesRequestedversion()
        {
            var p = factory.Make(110);
            Assert.That(p, Is.InstanceOf<TSql110Parser>());

            p = factory.Make(130);
            Assert.That(p, Is.InstanceOf<TSql130Parser>());

            p = factory.Make(140);
            Assert.That(p, Is.InstanceOf<TSql140Parser>());

            p = factory.Make(150);
            Assert.That(p, Is.InstanceOf<TSql150Parser>());
        }

        [Test]
        public void TestTSqlParserFactoryFailsOnBadCompatLevel()
        {
            try
            {
                factory.Make(333);
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
            string sql = @"
            declare 
            @dt_json        NVARCHAR(MAX)
            
            SET @dt_json =
            (
                SELECT
                    'test' AS command
                    , (SELECT TOP(1) acc_code, sys_name FROM dbo.acc_access WHERE id > 1 ORDER BY id FOR JSON PATH) AS packet
                FOR JSON PATH);
            ";

            var linter = factory.Make(150);
            linter.Parse(new StringReader(sql), out IList<ParseError> err);

            Assert.That(err, Is.Empty);
        }
    }
}
