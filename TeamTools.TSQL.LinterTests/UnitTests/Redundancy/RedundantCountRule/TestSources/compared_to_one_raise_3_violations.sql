SELECT 1
FROM dbo.foo
WHERE (SELECT COUNT(*) FROM dbo.bar) >= 1
GO

SELECT 1
FROM dbo.foo
WHERE (SELECT (COUNT(*)) FROM dbo.bar) < (+1)
GO

SELECT 1
FROM dbo.foo
WHERE (SELECT (1)) > (SELECT COUNT(*) FROM dbo.bar)
GO
