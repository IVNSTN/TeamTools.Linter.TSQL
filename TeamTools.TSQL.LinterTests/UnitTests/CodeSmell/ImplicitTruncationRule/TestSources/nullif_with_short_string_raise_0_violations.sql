CREATE PROC dbo.foo
    @value VARCHAR(20)
AS
BEGIN
    SET @value = NULLIF(@value, 'none');
END;
GO
