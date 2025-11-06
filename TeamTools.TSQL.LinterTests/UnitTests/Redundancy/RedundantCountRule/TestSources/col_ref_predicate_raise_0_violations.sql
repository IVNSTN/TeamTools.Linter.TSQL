SELECT 1
FROM dbo.foo
WHERE foo.total >= (SELECT COUNT(*) FROM dbo.bar)
GO
