SELECT 1
FROM dbo.foo
WHERE (SELECT COUNT(*) * 2 FROM dbo.bar) >= 1
GO
