DECLARE @arg TIME

EXEC sp_executesql
    N'SELECT @param',
    N'@time TIME, @param INT',
    @time = @arg -- named

EXEC sp_executesql
    N'SELECT @param',
    N'@time TIME, @param INT',
    @arg, -- positional
    NULL
