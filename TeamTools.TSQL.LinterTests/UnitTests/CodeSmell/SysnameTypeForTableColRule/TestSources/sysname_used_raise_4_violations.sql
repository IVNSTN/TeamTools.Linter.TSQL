CREATE TABLE foo (
    name SYSNAME NULL
)

CREATE TABLE #foo (
    name SYSNAME NOT NULL
)

DECLARE @foo TABLE (
    name SYSNAME NULL
)
GO
CREATE TYPE dbo.bar AS TABLE
(
    name    SYSNAME NOT NULL,
    dt      DATETIME,
    computed_col AS (dt-1)
)
