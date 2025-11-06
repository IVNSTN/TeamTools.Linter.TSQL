CREATE FUNCTION dbo.foo(@x INT)
RETURNS INT
AS
BEGIN
    SELECT @x = t.ID
    FROM dbo.far

    RETURN @x;
END
GO

CREATE PROCEDURE dbo.bar
    @x INT
AS
BEGIN
    DECLARE @a INT
    SET @x = @a;

    SELECT @x;
END
