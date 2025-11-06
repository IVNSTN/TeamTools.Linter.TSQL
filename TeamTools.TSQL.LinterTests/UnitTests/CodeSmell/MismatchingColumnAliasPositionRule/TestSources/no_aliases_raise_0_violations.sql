WITH cte AS (
    SELECT a, b, c
    FROM dbo.foo
)
SELECT *
FROM cte c
INNER JOIN (
    SELECT e, f, g
    FROM dbo.bar
) b
ON b.e = c.b
