-- compatibility level min: 110
INSERT dbo.foo(id, title)
SELECT id, title
FROM dbo.bar
ORDER BY title ASC
OFFSET 10 ROWS FETCH NEXT 100 ROWS ONLY
