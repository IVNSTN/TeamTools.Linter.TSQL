CREATE PROCEDURE dbo.GlobalTempCleanupAtEnd
AS
BEGIN
    CREATE TABLE ##GlobalTemp (Id INT);
    INSERT INTO ##GlobalTemp (Id) VALUES (1);
    DROP TABLE ##GlobalTemp;
END;
