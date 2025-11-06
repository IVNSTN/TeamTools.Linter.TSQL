CREATE FUNCTION dbo.foo(@x INT, @date DATE)
RETURNS INT
AS
BEGIN
    DECLARE @a INT = @x
    SET @date = CAST(ISNULL(@date, GETDATE()) AS DATE);
    RETURN @a;
END
GO

CREATE PROCEDURE dbo.bar
    @x INT
    , @StartDate DATE
AS
BEGIN
    DECLARE @a INT

    SELECT @x = @a + (@x)

    SELECT @a;

    IF ((@StartDate) IS NULL OR 1=1)
        SET @StartDate = GETDATE();
END
GO

CREATE PROCEDURE dbo.bar
    @x INT = NULL
AS
BEGIN
    EXEC sp_executesql N'SELECT @var', N'@var INT', @x

    RETURN @x;
END
GO

CREATE PROCEDURE dbo.bar
    @x INT = NULL
AS
BEGIN
    SET @x += 1

    RETURN @x;
END
GO

CREATE PROCEDURE dbo.bar
    @x INT
AS
BEGIN
    SET @b = (CASE WHEN (1=0) THEN NULL ELSE (@x) END)

    SET @x = NULL;

    RETURN @x;
END
