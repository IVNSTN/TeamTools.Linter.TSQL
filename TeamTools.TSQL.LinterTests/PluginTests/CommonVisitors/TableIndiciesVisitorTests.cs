using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(TableIndicesVisitor))]
    public sealed class TableIndiciesVisitorTests
    {
        private TableIndicesVisitor visitor;
        private MockLinter linter;
        private IList<ParseError> err;

        [SetUp]
        public void SetUp()
        {
            visitor = new TableIndicesVisitor();
            linter = MockLinter.MakeDefaultLinter();
            err = null;
        }

        [Test]
        public void TestIndiciesVisitorFindsIdxAndPk()
        {
            linter.Lint(
            @"
                CREATE TABLE dbo.foo
                (
                    id int,
                    name varchar(100),
                    dt datetime2(0),
                    ver TIMESTAMP NOT NULL
                    , index ix_bar (id)
                    , constraint pk primary key nonclustered (dt)
                    , constraint uq_name unique (name)
                )
                GO
                CREATE UNIQUE CLUSTERED INDEX ix_zar ON  dbo.foo(id)
                ON [PRIMARY]
            ", out err).Accept(visitor);

            Assert.That(string.Join(Environment.NewLine, err.Select(e => e.Message)), Is.Empty, "failed parsing");
            Assert.That(visitor.Table, Is.Not.Null, "table not null");
            Assert.That(visitor.Table.SchemaObjectName.BaseIdentifier.Value, Is.EqualTo("foo"), "table name ok");
            Assert.That(visitor.Indices, Is.Not.Null, "indicies not null");
            Assert.That(visitor.Indices, Has.Count.EqualTo(3), "index count ok");
            Assert.That(visitor.Indices.Any(idx => idx.Name.Value.Equals("ix_bar", StringComparison.OrdinalIgnoreCase)), Is.True, "inline index ok");
            Assert.That(visitor.Indices.Any(idx => idx.Name.Value.Equals("ix_zar", StringComparison.OrdinalIgnoreCase)), Is.True, "separate index ok");
            Assert.That(visitor.Indices.Any(idx => idx.Name.Value.Equals("uq_name", StringComparison.OrdinalIgnoreCase)), Is.False, "unique non pk ignored");
            Assert.That(visitor.Indices.Any(idx => idx.Name.Value.Equals("pk", StringComparison.OrdinalIgnoreCase)), Is.True, "pk ok");
        }
    }
}
