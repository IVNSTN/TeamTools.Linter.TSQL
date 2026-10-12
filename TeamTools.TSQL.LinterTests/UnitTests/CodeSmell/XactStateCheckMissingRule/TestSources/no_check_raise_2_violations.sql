BEGIN TRAN

BEGIN TRY
    UPDATE foo SET upd = 1
END TRY
BEGIN CATCH
    -- entire transaction could be closed by now
    ROLLBACK TRAN
END CATCH

-- transaction may be inactive or uncommittable
SAVE TRAN trn

UPDATE foo SET upd = 1
GO
BEGIN TRAN

BEGIN TRY
    UPDATE foo SET upd = 1
END TRY
BEGIN CATCH
    --
END CATCH

-- transaction may be inactive or uncommittable
UPDATE foo SET upd = 1
