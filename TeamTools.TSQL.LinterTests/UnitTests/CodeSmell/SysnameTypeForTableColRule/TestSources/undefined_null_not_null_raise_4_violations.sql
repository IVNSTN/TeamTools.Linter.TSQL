CREATE TABLE foo (
    name SYSNAME,
    dt DATETIME
)

CREATE TABLE #foo (
    name SYSNAME,
    dt DATETIME
)

DECLARE @foo TABLE (
    name SYSNAME,
    dt DATETIME
)
GO
CREATE TYPE dbo.bar AS TABLE
(
    name    SYSNAME
)
