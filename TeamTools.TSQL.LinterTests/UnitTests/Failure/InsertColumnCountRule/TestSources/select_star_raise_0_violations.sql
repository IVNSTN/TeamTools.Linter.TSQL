-- there is a separate rule for catching SELECT *
INSERT dbo.foo(a, b, c)
SELECT * FROM dbo.bar
