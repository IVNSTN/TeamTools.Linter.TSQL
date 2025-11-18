EXEC dbo.foo

SELECT t.col FROM dbo.tbl
GO

CREATE FUNCTION foo.bar()
RETURNS dbo.my_type
BEGIN
    RETURN 1
END
GO

-- schema omitted
SELECT * FROM tempdb..#mytbl
