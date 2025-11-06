DECLARE @foo INT = dbo.my_fn()

IF CAST(@foo AS VARCHAR(10)) = '123'
    SELECT CONVERT(BIGINT, @foo)

DECLARE @foo CHAR(2), @bar VARCHAR(10), @i INT

-- implicit conversions are ignored
SELECT CONCAT(@foo, @i, @bar)
