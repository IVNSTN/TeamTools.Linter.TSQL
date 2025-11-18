-- compatibility level min: 110
ALTER PROCEDURE dbo.foo
AS
BEGIN
    BEGIN TRY
        -- this is not a "retrowing"
        THROW 50000, 'asdf', 1;
    END TRY
    BEGIN CATCH
    END CATCH
END
