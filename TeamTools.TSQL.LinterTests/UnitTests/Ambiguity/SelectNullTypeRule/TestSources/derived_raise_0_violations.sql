SELECT * FROM bar
INNER JOIN
(
    SELECT
        foo.title,
        NULL as start_time
    FROM foo
) far
ON id = id
