SELECT 1
FROM dbo.foo
WHERE foo.total >= (SELECT COUNT(+1) FROM dbo.bar)
GO

SELECT 1
FROM dbo.foo
WHERE 1 > (SELECT COUNT(id) FROM dbo.bar)
GO
