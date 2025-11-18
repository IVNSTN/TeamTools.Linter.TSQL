CREATE PROC dbo.foo
    @bar INT = NULL
AS
BEGIN
    PRINT @bar;
END;
