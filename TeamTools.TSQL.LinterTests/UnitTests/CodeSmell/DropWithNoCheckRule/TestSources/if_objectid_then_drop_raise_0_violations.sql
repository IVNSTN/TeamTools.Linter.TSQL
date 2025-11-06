IF OBJECT_ID('dbo.tbl', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.tbl;
END;
GO

IF OBJECT_ID('dbo.foo', 'P') IS NOT NULL
    DROP PROC dbo.foo;
GO

IF ((OBJECT_ID('tbl', 'U') IS NOT NULL)) -- dbo omitted
BEGIN
    DROP TABLE dbo.tbl;
END;
GO

IF NOT OBJECT_ID('dbo.foo', 'P') IS NULL -- expression reversed and negated
    DROP PROC foo; -- dbo omitted
GO
