SELECT a, @var, 1
FROM dbo.foo

INTERSECT

SELECT d, e, 0
FROM dbo.bar
GO

SELECT a, @var, 'a'
FROM dbo.foo

EXCEPT

SELECT d, e, 'b'
FROM dbo.bar
