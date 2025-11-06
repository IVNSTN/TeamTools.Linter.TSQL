DECLARE @foo INT = dbo.my_fn()

SELECT CAST(@foo AS INT) -- 1
PRINT CONVERT(INT, @foo) -- 2
