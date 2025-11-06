CREATE PROCEDURE dbo.bar
    @c INT OUTPUT,
    @d BIT OUTPUT,
    @e MONEY OUTPUT
AS
BEGIN
    DECLARE @f INT;
    SET @f = @c;
    SELECT @d;

    EXEC sp_executesql N'', 'TEST', @e;
END
