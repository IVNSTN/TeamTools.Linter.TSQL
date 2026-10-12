CREATE PROCEDURE dbo.bar
    @x   INT OUTPUT
    , @y INT
AS
BEGIN
    SET @x = @y;
END
GO

-- output only
CREATE PROCEDURE dbo.bar
    @x   INT OUTPUT
AS
BEGIN
    SET @x = 1;
END
