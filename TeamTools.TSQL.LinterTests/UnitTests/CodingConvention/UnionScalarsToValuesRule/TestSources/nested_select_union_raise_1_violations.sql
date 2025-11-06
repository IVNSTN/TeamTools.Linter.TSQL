SELECT a
FROM foo
INNER JOIN
(
    SELECT 1 AS id
    UNION ALL
    SELECT 2
    UNION ALL
    SELECT 3
) AS t
ON foo.id = t.id
