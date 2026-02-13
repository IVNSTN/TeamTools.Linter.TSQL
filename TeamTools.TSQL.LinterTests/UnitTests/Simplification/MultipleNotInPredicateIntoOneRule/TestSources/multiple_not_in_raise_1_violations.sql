-- ... => category_id  NOT IN (1, 2, 3, 4, 5)
SELECT *
FROM dbo.foo
WHERE category_id NOT IN (1, 2, 3)
    AND category_id NOT IN (4, 5)
