DECLARE @foo CHAR(5) = N'123'
    , @bar VARCHAR(MAX)

SET @bar = @foo + 'abcdef'

PRINT @bar
GO

CREATE FUNCTION dbo.far(@id INT)
RETURNS VARCHAR(10)
AS
BEGIN
    RETURN '   '
END
GO
