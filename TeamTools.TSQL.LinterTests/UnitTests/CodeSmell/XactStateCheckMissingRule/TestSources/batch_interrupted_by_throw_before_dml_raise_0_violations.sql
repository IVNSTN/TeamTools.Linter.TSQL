-- compatibility level min: 110
BEGIN TRAN

BEGIN TRY
    UPDATE foo SET upd = 1
END TRY
BEGIN CATCH;
    -- throw means quit
    THROW;
    ROLLBACK TRAN
END CATCH
