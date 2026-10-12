CREATE PROCEDURE dbo.TempTableInDynamicSql
AS
BEGIN
    EXEC sp_executesql N'TRUCATE TABLE #TempData';
END;
