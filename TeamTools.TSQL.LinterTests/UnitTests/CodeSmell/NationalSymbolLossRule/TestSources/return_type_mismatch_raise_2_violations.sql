CREATE FUNCTION dbo.sun(@dummy INT)
RETURNS VARCHAR(10)
AS
BEGIN
    DECLARE @far NCHAR(5) = N'☀️'
    IF 1 = 0
        RETURN @far             -- 1

    RETURN N'☀️☀️☀️'           -- 2
END
GO
