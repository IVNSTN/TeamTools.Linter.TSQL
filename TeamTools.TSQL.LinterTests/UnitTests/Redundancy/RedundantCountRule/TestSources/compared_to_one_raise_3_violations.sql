SELECT 1
FROM dbo.foo
WHERE (SELECT COUNT(*) FROM dbo.bar) >= 1
GO

SELECT 1
FROM dbo.foo
WHERE (SELECT +(COUNT(1+(1))) FROM dbo.bar) < (+1)
GO

SELECT 1
FROM dbo.foo
WHERE (SELECT (0)) !< (SELECT COUNT(*) FROM dbo.bar)
GO

