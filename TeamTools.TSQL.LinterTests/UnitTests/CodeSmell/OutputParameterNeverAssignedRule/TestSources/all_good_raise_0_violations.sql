CREATE PROCEDURE dbo.foo
    @a INT,
    @b BIT
AS;
GO
CREATE PROCEDURE dbo.bar
    @c INT OUTPUT,
    @d BIT OUTPUT,
    @e MONEY OUTPUT
AS
BEGIN
    SET @c = 1;

    SELECT @d = 2;

    EXEC sp_executesql N'', 'TEST', @e OUTPUT;
END
