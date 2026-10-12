CREATE PROCEDURE dbo.SetNocountInMiddleNoViolation
AS
BEGIN
    CREATE TABLE #TempData (Id INT);
    SET NOCOUNT OFF;
    INSERT INTO #TempData (Id) VALUES (1);
    INSERT INTO #TempData (Id) VALUES (2);
END;
GO

CREATE PROCEDURE dbo.SetNocountInMiddleNoViolation
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * FROM dbo.src
END;
GO
