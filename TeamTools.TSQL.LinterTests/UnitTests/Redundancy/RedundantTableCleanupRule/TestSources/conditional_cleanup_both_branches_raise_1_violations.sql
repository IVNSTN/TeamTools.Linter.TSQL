CREATE PROCEDURE dbo.ConditionalCleanupBothBranches
AS
BEGIN
    CREATE TABLE #TempData (Id INT);
    INSERT INTO #TempData (Id) VALUES (1);

    IF @Flag = 1
        TRUNCATE TABLE #TempData;
    ELSE
        TRUNCATE TABLE #TempData;

    PRINT 'done'

    RETURN 1
END;
