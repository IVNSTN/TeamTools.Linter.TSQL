CREATE TABLE dbo.foo
(
    col DECIMAL
)

CREATE TABLE #foo
(
    col DECIMAL(10)
)

DECLARE @bar TABLE
(
    col DECIMAL
)

CREATE TYPE dbo.bar AS TABLE
(
    col DECIMAL(10) NOT NULL
)
