WITH cte (title, id) AS
(
    SELECT
        foo.title,
        1
    FROM foo
)
SELECT
    title, id, 1 + 1
FROM cte
INNER JOIN
(
    SELECT
        foo.title,
        GETDATE() as start_time
    FROM foo
) far
on cte.id = far.id

UPDATE t SET
    lastmod = GETDATE()
    OUTPUT
        INSERTED.lastmod,
        DELETED.id,
        1
FROM tmp AS t
