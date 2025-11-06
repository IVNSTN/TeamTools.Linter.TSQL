CREATE TABLE dbo.foo
(
    col FLOAT
)

CREATE TABLE #foo
(
    col FLOAT(10)
)

DECLARE @bar TABLE
(
    col FLOAT
)

CREATE TYPE dbo.bar AS TABLE
(
    col FLOAT(10) NOT NULL
)
