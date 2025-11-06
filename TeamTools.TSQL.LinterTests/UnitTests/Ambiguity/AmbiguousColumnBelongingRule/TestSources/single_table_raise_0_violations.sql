UPDATE t SET
    title = new_title
FROM dbo.foo AS t
WHERE id = @id

;WITH cte AS (
    SELECT parent_id, title
    FROM dbo.bar
)
SELECT
    id
    , parent_id
    , title AS child_title
FROM cte
WHERE title LIKE 'asdf%'
