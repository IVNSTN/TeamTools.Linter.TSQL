CREATE TABLE foo (
    name    NVARCHAR(128) NOT NULL,
    dt      DATETIME,
    computed_col AS (dt-1)
)

CREATE TABLE #foo (
    name    NVARCHAR(128) NOT NULL,
    dt      DATETIME,
    computed_col AS (dt-1)
)

DECLARE @foo TABLE (
    name    NVARCHAR(128) NOT NULL,
    dt      DATETIME,
    computed_col AS (dt-1)
)
GO
CREATE TYPE dbo.bar AS TABLE
(
    name    NVARCHAR(128) NOT NULL,
    dt      DATETIME,
    computed_col AS (dt-1)
)
