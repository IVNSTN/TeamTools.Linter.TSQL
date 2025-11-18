-- short lists
IF @a IN (1, 2, 3, 4, 5)
    SELECT 1
    FROM foo.bar
    WHERE jar NOT IN (car, far)

SELECT id
FROM dbo.foo
WHERE parent_id IN (SELECT id FROM @root_nodes) -- subquery instead of values
