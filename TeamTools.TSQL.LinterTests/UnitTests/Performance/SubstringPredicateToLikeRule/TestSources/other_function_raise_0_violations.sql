SELECT f.id
FROM dbo.foo f
WHERE REPLACE(f.title, 1, 1) = 'asdf'
