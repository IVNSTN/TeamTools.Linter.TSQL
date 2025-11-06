SELECT TOP(10) *
FROM dbo.foo

SELECT TOP 5 PERCENT a, b, c
FROM dbo.bar

SELECT TOP(@n) id
FROM dbo.far

SELECT TOP(@n) PERCENT r.id
FROM dbo.zar AS r
