SELECT
    CASE
        WHEN t.[asdf] IS NULL THEN
            1
        WHEN (dbo.fn(t.[asdf]) IS NOT NULL) THEN
            2
        ELSE 0
    END AS res
FROM t

SELECT 1
WHERE col NOT IN
    (
        1
        , 2
    )

SELECT 1
WHERE col NOT IN (SELECT a FROM b)

IF NULLIF(@foo, '') IS NOT NULL
    PRINT 'NOT NULL'

FETCH NEXT FROM cr INTO @var

FETCH cr
