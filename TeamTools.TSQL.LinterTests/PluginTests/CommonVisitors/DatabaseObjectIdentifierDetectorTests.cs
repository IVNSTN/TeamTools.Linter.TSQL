using Microsoft.SqlServer.TransactSql.ScriptDom;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.TSQL.Linter.Routines;

namespace TeamTools.TSQL.LinterTests
{
    [Category("Linter.TSQL.Routines")]
    [TestOf(typeof(DatabaseObjectIdentifierDetector))]
    public sealed class DatabaseObjectIdentifierDetectorTests
    {
        private MockLinter linter;
        private IList<ParseError> err;

        [SetUp]
        public void SetUp()
        {
            linter = MockLinter.MakeDefaultLinter();
            err = null;
        }

        [Test]
        public void TestDatabaseObjectIdentifierDetectorDefinitionOnlyMode()
        {
            List<string> ids = new List<string>();

            var rule = new DatabaseObjectIdentifierDetector(
                (id) => { ids.Add(string.Join(".", id.Identifiers.Select(i => i.Value))); },
                true);

            linter.Lint(
            @"
                CREATE VIEW dbo.foo_view AS select 1
                GO
                CREATE TRIGGER dbo.foo_trigger ON dbo.foo AFTER DELETE AS RETURN;
                GO
                CREATE PARTITION SCHEME foo_part_scheme AS PARTITION udp_datetime2_PF TO ([PRIMARY])
                GO
                DROP TABLE dbo.drop_foo_table;
                GO
                CREATE FUNCTION dbo.foo_fn() RETURNS INT AS BEGIN RETURN 1 END;
                GO
                DECLARE foo_cr CURSOR FAST_FORWARD FOR
                    SELECT a, b
                    FROM c
                GO
                ALTER TABLE bar ADD CONSTRAINT DF_far DEFAULT (1) FOR far;
                GO
                CREATE SCHEMA foo_schema
                GO
                CREATE SERVICE foo_svc ON QUEUE foo.queue
                GO
                CREATE MESSAGE TYPE foo_msg VALIDATION = NONE
                GO
                ;with foo_cte as (select 1 as id)
                select * from foo_cte
                GO
                CREATE INDEX foo_idx on bar(name);
                GO
                CREATE TABLE foo_tbl
                (
                    foo_col INT,
                    INDEX foo_idx_def ON foo_col
                )
                GO
                CREATE PROC foo_proc AS;
                GO
                SELECT foo_col_alias as foo_col_alias
                FROM foo_tbl_alias as foo_tbl_alias
                GO
            ", out err).Accept(rule);

            Assert.That(string.Join(Environment.NewLine, err.Select(e => e.Message)), Is.Empty, "failed parsing");

            Assert.That(ids, Does.Contain("dbo.foo_view"), "view");
            Assert.That(ids, Does.Contain("dbo.foo_trigger"), "trigger");
            // not schemaname
            Assert.That(ids, Does.Not.Contain("foo_part_scheme"), "partition scheme");
            // because def only
            Assert.That(ids, Does.Not.Contain("drop_foo_table"), "drop table");
            Assert.That(ids, Does.Contain("dbo.foo_fn"), "fn");
            // not schemaname
            Assert.That(ids, Does.Not.Contain("foo_cr"), "cursor");
            Assert.That(ids, Does.Not.Contain("DF_far"), "constraint");
            Assert.That(ids, Does.Not.Contain("foo_idx"), "index stmt");
            Assert.That(ids, Does.Not.Contain("foo_idx_def"), "index def");
            Assert.That(ids, Does.Not.Contain("foo_schema"), "schema");
            Assert.That(ids, Does.Not.Contain("foo_svc"), "service");
            Assert.That(ids, Does.Not.Contain("foo_msg"), "message type");
            Assert.That(ids, Does.Not.Contain("foo_cte"), "cte");

            Assert.That(ids, Does.Contain("foo_tbl"), "table");

            Assert.That(ids, Does.Not.Contain("foo_col"), "column");

            Assert.That(ids, Does.Contain("foo_proc"), "procedure");
            // because def only
            Assert.That(ids, Does.Not.Contain("foo_tbl_alias"), "tbl alias");
            Assert.That(ids, Does.Not.Contain("foo_col_alias"), "column alias");
        }

        [Test]
        public void TestDatabaseObjectIdentifierDetectorGetAllMode()
        {
            List<string> ids = new List<string>();

            var rule = new DatabaseObjectIdentifierDetector(
                (id, name) =>
                {
                    ids.Add(name);
                });

            linter.Lint(
            @"
                CREATE VIEW dbo.foo_view AS select 1
                GO
                CREATE TRIGGER dbo.foo_trigger ON dbo.foo AFTER DELETE AS RETURN;
                GO
                CREATE PARTITION SCHEME foo_part_scheme AS PARTITION udp_datetime2_PF TO ([PRIMARY])
                GO
                DROP TABLE dbo.drop_foo_table;
                GO
                CREATE FUNCTION dbo.foo_fn() RETURNS INT AS BEGIN RETURN 1 END;
                GO
                DECLARE foo_cr CURSOR FAST_FORWARD FOR
                    SELECT a, b
                    FROM c
                GO
                ALTER TABLE bar ADD CONSTRAINT DF_far DEFAULT (1) FOR far;
                GO
                CREATE SCHEMA foo_schema
                GO
                CREATE SERVICE foo_svc ON QUEUE foo.queue
                GO
                CREATE MESSAGE TYPE foo_msg VALIDATION = NONE
                GO
                ;with foo_cte as (select 1 as id)
                select * from foo_cte
                GO
                CREATE INDEX foo_idx on bar(name);
                GO
                CREATE TABLE foo_tbl
                (
                    foo_col INT,
                    INDEX foo_idx_def ON foo_col
                )
                GO
                CREATE PROC foo_proc AS;
                GO
                UPDATE goo SET 
                    name = foo_tbl_alias.foo_col_alias
                FROM (
                    select foo_tbl_alias.foo_col_alias as foo_col_alias
                    from foo_tbl_alias as foo_tbl_alias
                ) as foo_tbl_alias
                INNER JOIN goo_tbl as goo
                    on goo.id = foo_tbl_alias.id
                GO
            ", out err).Accept(rule);
            Assert.That(string.Join(Environment.NewLine, err.Select(e => e.Message)), Is.Empty, "failed parsing");

            Assert.That(ids, Does.Contain("foo_view"), "view");
            Assert.That(ids, Does.Contain("foo_trigger"), "trigger");
            Assert.That(ids, Does.Contain("foo_part_scheme"), "partition scheme");
            Assert.That(ids, Does.Contain("drop_foo_table"), "drop table");
            Assert.That(ids, Does.Contain("foo_fn"), "fn");
            Assert.That(ids, Does.Contain("foo_cr"), "cursor");
            Assert.That(ids, Does.Contain("DF_far"), "constraint");
            Assert.That(ids, Does.Contain("foo_idx"), "index stmt");
            Assert.That(ids, Does.Contain("foo_idx_def"), "index def");
            Assert.That(ids, Does.Contain("foo_schema"), "schema");
            Assert.That(ids, Does.Contain("foo_svc"), "service");
            Assert.That(ids, Does.Contain("foo_msg"), "message type");
            Assert.That(ids, Does.Contain("foo_cte"), "cte");
            Assert.That(ids, Does.Contain("foo_tbl"), "table");
            Assert.That(ids, Does.Contain("foo_col"), "column");
            Assert.That(ids, Does.Contain("foo_proc"), "procedure");
            Assert.That(ids, Does.Contain("foo_tbl_alias"), "tbl alias");
            Assert.That(ids, Does.Contain("foo_col_alias"), "column alias");
        }
    }
}
