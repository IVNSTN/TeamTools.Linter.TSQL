CREATE TRIGGER dbo.TriggerCleanupAtEnd
ON dbo.SomeTable
AFTER INSERT, UPDATE
AS
BEGIN
    INSERT INTO #TempLog (Action) VALUES ('Modified');
    SET NOCOUNT OFF;
END;
