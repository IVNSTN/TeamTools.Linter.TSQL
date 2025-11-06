WITH foo AS (
    SELECT a, b, c
    FROM dbo.bar
)
SELECT *
FROM foo
GO

WITH foo AS (
    SELECT a, b, c
    FROM dbo.bar
)
SELECT *
FROM foo
ORDER BY 1
