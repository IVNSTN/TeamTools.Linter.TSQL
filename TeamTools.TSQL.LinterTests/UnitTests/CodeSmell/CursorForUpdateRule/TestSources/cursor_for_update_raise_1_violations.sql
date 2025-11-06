DECLARE @cr CURSOR

SET @cr = CURSOR FOR
SELECT id, calc_value
FROM dbo.foo
FOR UPDATE OF calc_value -- here
