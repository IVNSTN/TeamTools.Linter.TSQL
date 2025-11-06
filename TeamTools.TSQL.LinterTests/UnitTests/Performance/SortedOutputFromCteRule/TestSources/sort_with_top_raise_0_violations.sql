WITH foo AS (
    SELECT TOP(10) a, b, c
    FROM dbo.bar
    ORDER BY a DESC
)
SELECT *
FROM foo
