IF OBJECT_ID('bar.tbl', 'U') IS NOT NULL -- 1
BEGIN
    DROP TABLE tbl;
END;
GO

IF OBJECT_ID('foo.foo', 'P') IS NOT NULL -- 2
    DROP PROC zoo.goo;
GO
