SELECT a, @var, 1
FROM dbo.foo

-- there is a separate rule for UNION
UNION

SELECT d, e, 0
FROM dbo.bar
GO
