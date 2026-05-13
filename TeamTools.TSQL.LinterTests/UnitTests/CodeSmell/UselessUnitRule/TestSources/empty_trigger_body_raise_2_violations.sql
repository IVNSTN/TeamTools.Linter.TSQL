CREATE TRIGGER dbo.tr ON dbo.bar AFTER INSERT AS
BEGIN
    -- same as nothing
    PRINT '';
END
GO

CREATE TRIGGER dbo.tr ON dbo.bar AFTER INSERT AS
BEGIN
    SET NOCOUNT ON

    -- nested begin-ends should be handled by another rule
    RETURN;
END
GO
