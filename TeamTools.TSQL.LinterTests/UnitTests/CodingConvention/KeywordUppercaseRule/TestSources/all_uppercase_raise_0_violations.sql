SET NOCOUNT ON;
DECLARE @a INT;
IF 1 <> 0
BEGIN
    SELECT TOP 1 1 AS [begin] FROM dbo.foo AS [catch]
    WHERE [user] <> cast(zar AS date)
    ORDER BY CASE WHEN upper([declare]) = [catch].[tran] THEN [catch].[exec] ELSE [end] END;
END;
BEGIN TRY
    EXECUTE AS USER = 'admin';
    EXEC dbo.test;
    REVERT;
END TRY
BEGIN CATCH
    ROLLBACK TRAN;
END CATCH;
RETURN;
