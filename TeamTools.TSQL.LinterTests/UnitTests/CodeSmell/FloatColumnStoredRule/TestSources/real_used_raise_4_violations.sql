CREATE TABLE dbo.foo
(
    col REAL
)

CREATE TABLE #foo
(
    col REAL
)

DECLARE @bar TABLE
(
    col REAL
)

CREATE TYPE dbo.bar AS TABLE
(
    col REAL NOT NULL
)
