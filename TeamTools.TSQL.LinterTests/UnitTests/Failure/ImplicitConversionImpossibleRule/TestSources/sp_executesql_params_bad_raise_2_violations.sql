DECLARE @arg TIME

EXEC sp_executesql
    N'SELECT @param',
    N'@time TIME, @param INT',
    @param = @arg -- named

EXEC sp_executesql
    N'SELECT @param',
    N'@time TIME, @param INT',
    NULL,
    @arg -- positional
