CREATE PROC dbo.foo
    @arg1 INT
    , @arg2 BIT = NULL
AS
BEGIN
    PRINT 'ok'
END
GO

CREATE FUNCTION dbo.bar(@id int)
RETURNS TABLE
AS
RETURN (
SELECT 1 AS id
)
GO
