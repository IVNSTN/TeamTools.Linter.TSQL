CREATE PROC dbo.foo
    @category_id INT
-- no recompile at proc level
AS
BEGIN
    SELECT *
    FROM dbo.bar
    WHERE category_id = @category_id
    OPTION (RECOMPILE)
END
GO

CREATE PROC dbo.foo
    @category_id INT
WITH RECOMPILE
AS
BEGIN
    SELECT *
    FROM dbo.bar
    WHERE category_id = @category_id
    -- no recompile at query level
END
GO

-- no recompile at all
CREATE PROC dbo.foo
    @category_id INT
WITH EXECUTE AS OWNER, ENCRYPTION
AS
BEGIN
    SELECT *
    FROM dbo.bar
    WHERE category_id = @category_id
    OPTION (HASH JOIN, MAXDOP 1)
END
GO

-- no options whatsoever
CREATE PROC dbo.foo
    @category_id INT
AS
BEGIN
    SELECT *
    FROM dbo.bar
    WHERE category_id = @category_id
END
GO
