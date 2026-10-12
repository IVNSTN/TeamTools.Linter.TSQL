
BEGIN TRAN

BEGIN TRY
    UPDATE foo SET upd = 1
END TRY
BEGIN CATCH
    IF XACT_STATE() = -1
        ROLLBACK TRAN
END CATCH

-- transaction may be active but uncommittable
IF XACT_STATE() = 1
    COMMIT TRAN

UPDATE foo SET upd = 1
