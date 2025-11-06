CREATE PROC dbo.foo
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @a INT;

    SET @a = 1;

    PRINT @a;

    RETURN 1;
END;
