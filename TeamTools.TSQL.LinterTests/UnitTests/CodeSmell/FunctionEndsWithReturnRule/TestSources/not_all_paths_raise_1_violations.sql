CREATE FUNCTION dbo.fn (@arg INT)
RETURNS INT
AS
BEGIN
    SET @arg += 1;

    IF @arg > 0
    BEGIN
        SET @arg -= 1; -- no return
    END
    ELSE
        RETURN 1; -- conditional
END;
