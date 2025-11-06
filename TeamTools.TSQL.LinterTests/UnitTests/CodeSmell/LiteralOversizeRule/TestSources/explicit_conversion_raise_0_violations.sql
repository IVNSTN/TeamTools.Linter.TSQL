CREATE PROC dbo.foo
    @expared      CHAR(1)
    , @event_text VARCHAR(8000)
AS
BEGIN
    SET @event_text = ISNULL('''' + CONVERT(VARCHAR(4), @expared) + '''', 'NULL');
END;
