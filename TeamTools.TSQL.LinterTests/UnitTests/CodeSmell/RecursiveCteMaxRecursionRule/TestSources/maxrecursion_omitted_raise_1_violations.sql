WITH cte_tree AS
(
    SELECT id, parent_id
    FROM dbo.bar
    WHERE parent_id IS NULL
    UNION ALL
    SELECT b.id, b.parent_id
    FROM cte_tree p
    INNER JOIN dbo.bar b
    ON b.parent_id = p.parent_id
)
SELECT f.*
FROM dbo.foo f
INNER JOIN cte e
ON e.id = f.id
-- OPTION (MAXRECURSION 10)
