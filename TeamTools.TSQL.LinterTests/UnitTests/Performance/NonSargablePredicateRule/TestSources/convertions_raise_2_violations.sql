SELECT *
FROM dbo.foo f
INNER JOIN bdo.bar b
ON b.id = CAST(f.id AS BIGINT)
WHERE TRY_CONVERT(DATE, f.num) > 0
