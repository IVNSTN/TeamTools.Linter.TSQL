SELECT f.id
FROM dbo.foo f
WHERE SUBSTRING(f.title, 1, 1) = 'A'
    OR SUBSTRING(f.title, 1, 1) = @b
