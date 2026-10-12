DECLARE @v SQL_VARIANT

-- here values will be compared as SQL_VARIANTs
IF @v IN (1, 2, 3)
    PRINT '1'

-- CASE output is SQL_VARIANT here
SELECT
    CASE 'asfd'
        -- the CAST itself is not reported but 'asdf' is compared to the 0 below
        -- after implicit conversion to SQL_VARIANT
        WHEN CAST(0 as SQL_VARIANT) THEN ''
        WHEN 0 THEN 'a'
    END

-- here 0 is converted to SQL_VARIANT
IF 0 < @v
    PRINT 0
