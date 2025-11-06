SELECT f.id
FROM dbo.foo f
WHERE SUBSTRING(f.title, 3, 1) = 'C'
