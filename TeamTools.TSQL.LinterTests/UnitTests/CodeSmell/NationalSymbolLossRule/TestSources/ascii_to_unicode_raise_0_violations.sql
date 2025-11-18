DECLARE @foo CHAR(5) = N'123'
    , @bar NVARCHAR(MAX)

SET @bar = @foo + 'abcdef'

PRINT @bar
GO

CREATE FUNCTION dbo.far(@id INT)
RETURNS NVARCHAR(10)
AS
BEGIN
    RETURN 'xxx'
END
GO
