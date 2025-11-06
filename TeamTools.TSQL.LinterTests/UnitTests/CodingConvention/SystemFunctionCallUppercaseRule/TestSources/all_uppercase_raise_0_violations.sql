SELECT
    UPPER(LOWER(LEFT(RIGHT(COALESCE(NULLIF('', ''), ISNULL('x', 'y')), 1), 1)))
    , ROUND(CAST(CONVERT(NUMERIC(10,2), CAST(CONVERT(NUMERIC(10,2), REPLACE('0', ' ', '')) AS NUMERIC(10,2))) as NUMERIC(10,2)), 0)
    , DATEADD(ms, DATEDIFF(y, GETDATE(), 0), SYSDATETIME())
    , user /* not handled */
    , year -- column
    , YEAR /* fn */ ()
