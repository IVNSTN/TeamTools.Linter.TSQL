INSERT dbo.foo(id, title)
SELECT TOP (10) id, title
FROM dbo.bar
ORDER BY title
