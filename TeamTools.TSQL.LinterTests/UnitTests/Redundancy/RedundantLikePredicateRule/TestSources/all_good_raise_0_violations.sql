SELECT *
FROM dbo.foo
WHERE name LIKE 'a%sdf'

SELECT *
FROM dbo.foo
WHERE name LIKE 'a[sd]f'

SELECT *
FROM dbo.foo
WHERE name LIKE 'a_f'

SELECT *
FROM dbo.foo
WHERE name LIKE @var


SELECT *
FROM dbo.foo
WHERE name = 'asdf'
