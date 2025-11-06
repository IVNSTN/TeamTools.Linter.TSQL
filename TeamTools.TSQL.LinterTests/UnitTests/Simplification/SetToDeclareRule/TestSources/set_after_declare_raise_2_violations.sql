CREATE PROC dbo.foo
AS
BEGIN
    SET NOCOUNT ON

    DECLARE @a INT,
            @b DATETIME


    SET @a = 2 -- 1

    BEGIN
        SELECT @b = GETDATE() -- 2
    END
END
