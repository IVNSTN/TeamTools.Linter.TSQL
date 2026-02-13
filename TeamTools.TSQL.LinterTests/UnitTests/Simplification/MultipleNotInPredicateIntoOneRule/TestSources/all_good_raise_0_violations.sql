SELECT *
FROM dbo.foo
WHERE category_id NOT IN (1, 2, 3)

SELECT *
FROM dbo.foo
WHERE category_id IN (1, 2, 3)


SELECT *
FROM dbo.foo
WHERE category_id NOT IN (1, 2, 3)
    AND title_id NOT IN (100, 200)

SELECT *
FROM dbo.foo
WHERE category_id NOT IN (1, 2, 3)
    OR category_id <> 200           -- OR cannot be collapsed to NOT IN

SELECT *
FROM dbo.foo
WHERE category_id IN (1, 2, 3)
    AND category_id <> 100
