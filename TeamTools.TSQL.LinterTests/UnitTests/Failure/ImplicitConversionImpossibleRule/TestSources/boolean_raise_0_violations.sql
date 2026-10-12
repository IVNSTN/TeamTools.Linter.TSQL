DECLARE @v SQL_VARIANT

-- here values will be compared as SQL_VARIANTs
IF @v IN (1, 2, 3)
    PRINT '1'
