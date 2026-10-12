-- compatibility level min: 110
BEGIN TRAN

BEGIN TRY
    UPDATE foo SET upd = 1
END TRY
BEGIN CATCH;
    THROW;
END CATCH

-- transaction may be uncommittable
-- but THROW means quit batch
INSERT bar
VALUES (1, 2)

-- error is reported once per possible error in a batch
COMMIT TRAN
