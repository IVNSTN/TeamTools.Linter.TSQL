using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(ColumnVisitor))]
    public sealed class ColumnVisitorTests
    {
        private ColumnVisitor visitor;
        private MockLinter linter;
        private IList<ParseError> err;

        [SetUp]
        public void SetUp()
        {
            visitor = new ColumnVisitor();
            linter = MockLinter.MakeLinter();
            err = null;
        }

        [Test]
        public void TestColumnVisitorDetectsAllColumns()
        {
            linter.Lint(
            @"
                CREATE TABLE dbo.foo
                (
                    id   int,
                    name varchar(100),
                    dt   date,
                    ver  TIMESTAMP NOT NULL
                )
            ", out err).Accept(visitor);

            Assert.That(string.Join(Environment.NewLine, err.Select(e => e.Message)), Is.Empty, "failed parsing");
            Assert.That(visitor.Columns, Is.Not.Null, "columns not null");
            Assert.That(visitor.Columns, Has.Count.EqualTo(4), "all col count");
        }

        [Test]
        public void TestColumnVisitorDetectsOnlyColumnsOfGivenTypes()
        {
            visitor = new ColumnVisitor(new string[] { "VARCHAR", "timestamp" });
            linter.Lint(
            @"
                CREATE TABLE dbo.foo
                (
                    id   int,
                    name varchar(100),
                    dt   date,
                    ver  TIMESTAMP NOT NULL
                )
            ", out err).Accept(visitor);

            Assert.That(string.Join(Environment.NewLine, err.Select(e => e.Message)), Is.Empty, "failed parsing");
            Assert.That(visitor.Columns, Is.Not.Null, "columns not null");
            Assert.That(visitor.Columns, Has.Count.EqualTo(2), "filtered col count");
        }

        [Test]
        public void TestColumnVisitorFindsNoColIfTypesDoNotMatch()
        {
            visitor = new ColumnVisitor(new string[] { "decimal", "char", "rowversion", "asdf" });
            linter.Lint(
            @"
                CREATE TABLE dbo.foo
                (
                    id   int,
                    name varchar(100),
                    dt   date,
                    ver  TIMESTAMP NOT NULL
                )
            ", out err).Accept(visitor);

            Assert.That(string.Join(Environment.NewLine, err.Select(e => e.Message)), Is.Empty, "failed parsing");
            Assert.That(visitor.Columns, Is.Not.Null, "columns not null");
            Assert.That(visitor.Columns, Is.Empty, "no cols");
        }

        [Test]
        public void TestColumnVisitorIgnoresComputedColumns()
        {
            visitor = new ColumnVisitor(new string[] { "int" });
            linter.Lint(
            @"
                CREATE TABLE dbo.foo
                (
                    id int,
                    volume as 123
                )
            ", out err).Accept(visitor);

            Assert.That(err, Is.Empty, "failed parsing 4");
            Assert.That(visitor.Columns, Is.Not.Null, "columns not null");
            Assert.That(visitor.Columns, Has.Count.EqualTo(1), "id and computed");
        }
    }
}
