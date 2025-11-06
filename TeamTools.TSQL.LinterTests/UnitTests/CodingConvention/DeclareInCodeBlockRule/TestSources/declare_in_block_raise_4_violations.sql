CREATE PROC dbo.foo
AS
BEGIN
    SET NOCOUNT ON;

    IF 1 = 0
    BEGIN
        DECLARE @a INT; -- 1
    END

    SET @a = 1;

    BEGIN TRY
        WHILE @a > 0
        BEGIN
            -- FIXME : this one will be reported twice: for TRY-CATCH and for WHILE
            DECLARE @tbl TABLE (id INT) -- 2
        END
    END TRY
    BEGIN CATCH
        DECLARE @err_msg VARCHAR(100) -- 3
    END CATCH

    RETURN 1;
END;
