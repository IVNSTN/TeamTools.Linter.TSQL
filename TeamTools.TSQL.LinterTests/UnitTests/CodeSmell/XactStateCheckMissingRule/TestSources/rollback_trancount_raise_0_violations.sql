
BEGIN TRAN

BEGIN TRY
    UPDATE foo SET upd = 1
END TRY
BEGIN CATCH
    -- nothing
END CATCH

-- rollback does not care if tran is uncommittable
IF @@TRANCOUNT > 0
    ROLLBACK TRAN
