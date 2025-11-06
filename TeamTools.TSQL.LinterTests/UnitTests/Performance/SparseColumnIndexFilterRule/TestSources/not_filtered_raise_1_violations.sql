CREATE TABLE dbo.foo
(
    bar INT SPARSE NULL
)
GO

CREATE INDEX ix ON dbo.foo(bar)
GO
