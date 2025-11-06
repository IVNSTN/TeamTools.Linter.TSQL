CREATE TABLE dbo.foo
(
    bar INT SPARSE NULL,
    far BIT NOT NULL
)
GO

-- filtered
CREATE INDEX ix ON dbo.foo(bar)
WHERE bar IS NOT NULL
GO

-- not sparse
CREATE INDEX ix2 ON dbo.foo(far)
GO

-- not in key
CREATE INDEX ix3 ON dbo.foo(far)
INCLUDE(bar)
GO

-- unknown table
CREATE INDEX ix4 ON dbo.unknown(far)
GO
