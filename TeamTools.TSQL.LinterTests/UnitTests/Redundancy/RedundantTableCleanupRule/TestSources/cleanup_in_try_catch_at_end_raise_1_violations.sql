CREATE PROCEDURE dbo.CleanupInTryCatchAtEnd
AS
BEGIN
    CREATE TABLE #TempData (Id INT);
    BEGIN TRY
        INSERT INTO #TempData (Id) VALUES (1);
    END TRY
    BEGIN CATCH
        TRUNCATE TABLE #TempData;
    END CATCH

    PRINT 'work logged'

    RETURN 0
END;
