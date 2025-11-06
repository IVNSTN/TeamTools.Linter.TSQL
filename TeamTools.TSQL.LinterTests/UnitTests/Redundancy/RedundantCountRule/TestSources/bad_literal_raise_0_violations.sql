SELECT 1
FROM dbo.foo
WHERE 'A' = (SELECT COUNT(*) FROM dbo.bar)
GO
