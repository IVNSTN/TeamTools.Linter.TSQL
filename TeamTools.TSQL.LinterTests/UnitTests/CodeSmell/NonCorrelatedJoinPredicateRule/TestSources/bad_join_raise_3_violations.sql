SELECT 1
FROM dbo.foo AS f
INNER JOIN dbo.car AS c
    ON b.group_id > 1           -- 1
WHERE c.id = b.id

SELECT 1
FROM dbo.foo AS f
INNER JOIN dbo.bar AS b
    ON b.enabled = @enabled     -- 2
LEFT JOIN dbo.far
    ON b.title != ''            -- 3

/* TODO : this should be reported as well
SELECT 1
FROM dbo.foo AS f
INNER JOIN dbo.bar AS b
    ON b.some_id = f.some_id
INNER JOIN dbo.car AS c
    ON b.group_id = f.group_id -- 4 "c" not mentioned
*/
