-- compatibility level min: 110
CREATE FUNCTION dbo.my_fn(@n int)
RETURNS TABLE
AS RETURN
SELECT TOP(10) id, title
FROM dbo.foo
ORDER BY id
OFFSET @n rows
FETCH NEXT 10 ROWS ONLY;
