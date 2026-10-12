DECLARE @v INT

IF @v IN (1, 2, 3)
    PRINT '1'

SELECT
    CASE
        WHEN 1=1 THEN CAST(0 as VARCHAR(10))
        WHEN 0=0 THEN 'a'
    END
