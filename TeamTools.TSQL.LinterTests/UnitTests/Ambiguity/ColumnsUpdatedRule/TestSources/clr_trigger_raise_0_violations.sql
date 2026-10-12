CREATE TRIGGER trg ON dbo.Table1
AFTER INSERT
AS EXTERNAL NAME [Assembly].[Class].[Method];
GO
