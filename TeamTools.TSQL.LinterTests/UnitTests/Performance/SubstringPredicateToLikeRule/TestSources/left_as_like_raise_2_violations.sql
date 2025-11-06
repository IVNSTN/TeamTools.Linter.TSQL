SELECT 1
FROM dbo.foo f
WHERE LEFT(f.title, 2) = 'asdf'
    OR LEFT(f.title, 2) = @str
