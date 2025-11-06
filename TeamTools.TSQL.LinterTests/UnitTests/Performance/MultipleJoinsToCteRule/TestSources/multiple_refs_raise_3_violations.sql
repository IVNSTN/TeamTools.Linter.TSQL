;WITH cte AS (SELECT 1 AS id)
UPDATE t
SET id = c.id + 1
FROM dbo.foo AS t
INNER JOIN cte AS c -- here
    ON c.id = b.id
LEFT JOIN dbo.bar AS b
    ON b.title = t.title
OUTER APPLY
(SELECT TOP 1 id FROM cte WHERE cte.id > b.id) AS cc -- here
WHERE NOT EXISTS (SELECT 1 FROM cte AS ccc WHERE ccc < t.id); -- here
