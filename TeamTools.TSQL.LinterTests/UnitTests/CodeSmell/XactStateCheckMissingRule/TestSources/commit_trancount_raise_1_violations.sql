
BEGIN TRAN

BEGIN TRY
    UPDATE foo SET upd = 1
END TRY
BEGIN CATCH
    -- nothing
END CATCH

-- transaction may be active but uncommittable
IF @@TRANCOUNT > 0
    COMMIT TRAN
