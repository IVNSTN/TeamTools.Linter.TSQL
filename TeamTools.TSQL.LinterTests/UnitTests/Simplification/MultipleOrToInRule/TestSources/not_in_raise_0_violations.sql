SELECT *
FROM dbo.foo
WHERE category_id NOT IN (1, 2, 3)
    OR category_id = @included_id
