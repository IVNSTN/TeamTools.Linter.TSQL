;WITH cte AS (
    SELECT 1 AS id
)
UPDATE t SET
    id = c.id+1
FROM dbo.foo AS t
LEFT JOIN cte AS c
ON c.id = t.id
