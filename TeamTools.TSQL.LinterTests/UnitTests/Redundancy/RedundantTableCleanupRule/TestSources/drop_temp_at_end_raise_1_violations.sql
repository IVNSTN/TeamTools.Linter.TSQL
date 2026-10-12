CREATE PROCEDURE dbo.DropTempAtEnd
AS
BEGIN
    CREATE TABLE #TempData (Id INT);
    INSERT INTO #TempData (Id) VALUES (1);
    DROP TABLE #TempData;
END;
