-- scalar
CREATE FUNCTION dbo.foo(@id INT)
RETURNS VARCHAR(100)
AS
BEGIN
    DECLARE @title VARCHAR(100)

    -- TODO : isn't this clearly a bad example?
    -- or such case should be detected by another rule?
    SELECT TOP 1 @title = title
    FROM dbo.bar
    WHERE id = @id

    RETURN @title
END
GO

-- inline
CREATE FUNCTION dbo.far()
RETURNS TABLE
AS
    RETURN
    (
        SELECT src.id
        FROM jar
        WHERE src.id > 0
    );
GO

-- case
CREATE FUNCTION dbo.foo(@id INT)
RETURNS VARCHAR(100)
AS
BEGIN
    RETURN CASE
        WHEN @id = 1 THEN 'A'
        WHEN @id = 2 THEN 'B'
        ELSE 'C'
    END
END
GO
