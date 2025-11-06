;WITH cte AS (SELECT id FROM dbo.bar) -- 1
DELETE f
FROM dbo.foo AS f;

WITH cte_a AS -- 2
(
    SELECT id, parent_id
    FROM dbo.bar
    WHERE parent_id IS NULL
    UNION ALL
    SELECT b.id, b.parent_id
    FROM dbo.bar AS b
    INNER JOIN cte_a AS c ON b.parent_id = c.id
)
     , cte_b AS (SELECT id, parent_id FROM cte_a) -- 3
SELECT d, b
FROM dbo.foo AS f
LEFT JOIN dbo.cte_b AS b ON b.id = f.id;
