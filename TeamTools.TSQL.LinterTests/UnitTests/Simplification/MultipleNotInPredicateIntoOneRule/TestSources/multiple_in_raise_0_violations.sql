-- ... => category_id  IN (1, 2, 3, 4, 5)
SELECT *
FROM dbo.foo
WHERE category_id IN (1, 2, 3)
    OR category_id IN (4, 5)
