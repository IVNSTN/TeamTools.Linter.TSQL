BEGIN TRAN

-- TRY block is ignored - if it fails then code in CATCH or after CATCH must verify XACT_STATE
-- before working with transaction or modifying data
BEGIN TRY
    SET XACT_ABORT ON
    RAISERROR ('asdf', 16, 1)

    INSERT bar
    VALUES (1, 2)
END TRY
BEGIN CATCH;
    --
END CATCH
