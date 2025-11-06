SELECT 'a', b
FROM dbo.foo
WHERE EXISTS (SELECT a, b, c
    FROM dbo.sub_query)

GO

SELECT 'a', b
FROM dbo.foo
WHERE NOT EXISTS (SELECT a, b, c
    FROM dbo.sub_query)
