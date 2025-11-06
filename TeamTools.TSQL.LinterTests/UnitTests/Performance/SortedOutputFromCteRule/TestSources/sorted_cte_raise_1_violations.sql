WITH foo AS (
    SELECT TOP 100 PERCENT a, b, c
    FROM dbo.bar
    ORDER BY a DESC
)
SELECT *
FROM foo
