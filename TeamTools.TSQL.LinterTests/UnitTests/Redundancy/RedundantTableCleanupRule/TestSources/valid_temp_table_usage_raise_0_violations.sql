CREATE PROCEDURE dbo.ValidTempTableUsage
AS
BEGIN
    CREATE TABLE #TempData (Id INT, Value VARCHAR(100));

    INSERT INTO #TempData (Id, Value) VALUES (1, 'test');
    TRUNCATE TABLE #TempData;
    INSERT INTO #TempData (Id, Value) VALUES (1, 'test');

    SELECT * FROM #TempData;
END;
