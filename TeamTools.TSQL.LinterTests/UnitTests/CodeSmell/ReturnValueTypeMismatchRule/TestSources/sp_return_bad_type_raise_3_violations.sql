CREATE PROC dbo.one
AS
BEGIN
    DECLARE @v XML

    -- only INT is supported
    RETURN 'adsf'       -- 1
    RETURN GETDATE()    -- 2
    RETURN @v           -- 3
END
GO
