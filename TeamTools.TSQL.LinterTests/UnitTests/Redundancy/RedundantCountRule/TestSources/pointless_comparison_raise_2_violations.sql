SELECT 1
FROM dbo.foo
WHERE (SELECT COUNT(*) FROM dbo.bar) < 0 -- count cannot be < 0
GO

SELECT 1
FROM dbo.foo
WHERE (SELECT COUNT((1)) FROM dbo.bar) >= 0 -- count will always be = 0 or > 0
GO
