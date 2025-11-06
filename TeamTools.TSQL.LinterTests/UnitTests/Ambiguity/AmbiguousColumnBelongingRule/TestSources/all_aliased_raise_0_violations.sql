UPDATE t SET
    title = b.new_title,
    lastmod = DATEADD(DAY, 1, GETDATE())
OUTPUT INSERTED.title
INTO @updated(title)
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = b.parent_id

;WITH cte (parent_id, title) AS (
    SELECT parent_id, title
    FROM dbo.bar
)
SELECT
    f.id
    , b.parent_id
    , b.title AS child_title
FROM dbo.foo AS f
INNER JOIN cte AS b
    ON b.parent_id = f.id
WHERE b.title LIKE 'asdf%'


INSERT dbo.foo(id, title, create_date)
VALUES (1, 'afds', DATEADD(DAY, 1, GETDATE()))
