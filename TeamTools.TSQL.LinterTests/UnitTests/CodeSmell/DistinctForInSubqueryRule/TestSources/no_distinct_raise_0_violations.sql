SELECT *
FROM dbo.foo
WHERE category_id IN (SELECT category_id FROM dbo.categories)

IF @a IN ('a', @aa)
    PRINT @a
