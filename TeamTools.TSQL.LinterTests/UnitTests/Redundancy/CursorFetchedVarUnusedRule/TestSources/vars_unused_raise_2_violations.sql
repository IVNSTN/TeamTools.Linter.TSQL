DECLARE cr CURSOR FAST_FORWARD FOR
    SELECT id, title
    FROM dbo.foo

OPEN cr

FETCH NEXT FROM cr
INTO
    @id,
    @title

-- @id value never accessed
PRINT @title

CLOSE cr
DEALLOCATE cr

PRINT @id -- refs after closing cursor are ignored
GO

DECLARE cr CURSOR FAST_FORWARD FOR
    SELECT id, title
    FROM dbo.foo

OPEN cr

FETCH NEXT FROM cr
INTO
    @id,
    @title

SET @id = 123 -- @id value is ignored
PRINT @title

CLOSE cr
DEALLOCATE cr
