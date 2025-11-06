SELECT a, @var, c
FROM dbo.foo

INTERSECT

SELECT d, e, 0
FROM dbo.bar
GO

SELECT a, @var, c
FROM dbo.foo

EXCEPT

SELECT d, e, 0
FROM dbo.bar
