SELECT *
FROM dbo.foo
WHERE category_id IN (SELECT DISTINCT category_id FROM dbo.categories)
