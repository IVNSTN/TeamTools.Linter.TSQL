-- compatibility level min: 110
ALTER PROCEDURE dbo.foo
AS
BEGIN
    BEGIN TRY
        SELECT 1
        RETURN 0;
    END TRY
    BEGIN CATCH
        -- retrowing error
        THROW;
    END CATCH
END
GO
CREATE PROCEDURE dbo.bar
AS
BEGIN
    BEGIN TRY
        SELECT 1
        RETURN 0;
    END TRY
    BEGIN CATCH
        -- retrowing error
        RAISERROR ('asdf', 16, 1);
    END CATCH
END
