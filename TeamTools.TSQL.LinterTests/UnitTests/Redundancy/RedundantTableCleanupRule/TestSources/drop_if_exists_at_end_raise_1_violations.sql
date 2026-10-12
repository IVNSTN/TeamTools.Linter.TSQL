CREATE PROCEDURE dbo.DropIfExistsAtEnd
AS
BEGIN
    CREATE TABLE #TempData (Id INT);
    INSERT INTO #TempData (Id) VALUES (1);
    IF EXISTS (SELECT 1 FROM #TempData)
        DROP TABLE #TempData;
    RETURN
END;
