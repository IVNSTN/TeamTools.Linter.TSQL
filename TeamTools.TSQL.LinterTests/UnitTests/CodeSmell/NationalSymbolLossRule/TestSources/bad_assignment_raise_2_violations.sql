DECLARE @foo NCHAR(5) = '☀️'
    , @bar NVARCHAR(MAX) = dbo.some_func()
    , @far VARCHAR(100)

SET @far = @foo         -- 1 precise value
SET @far = @bar         -- 2 unknown unicode value
