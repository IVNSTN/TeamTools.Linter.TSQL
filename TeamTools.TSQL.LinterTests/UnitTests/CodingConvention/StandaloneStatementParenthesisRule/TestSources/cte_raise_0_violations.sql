WITH cte (c, a, b) AS
(
    SELECT a, b, c
    FROM dbo.foo
)
SELECT *
FROM cte c
