SELECT 1
FROM dbo.foo
WHERE (SELECT MAX(bar.x) FROM dbo.bar) >= 1
GO

SELECT 1
FROM dbo.foo
-- wont work but will parse
WHERE (SELECT x, y FROM dbo.bar) >= 1
GO
