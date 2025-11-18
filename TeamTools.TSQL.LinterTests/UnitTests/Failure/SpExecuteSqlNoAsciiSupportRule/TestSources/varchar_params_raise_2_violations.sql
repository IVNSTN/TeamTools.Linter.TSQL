declare @script NCHAR(100) = 'select 1',
    @args CHAR(100) = '@foo VARCHAR(100)'

exec sp_executesql @script,
    @args -- 1
GO

sp_executesql  N'SELECT @arg',
    '@arg INT'          -- 2
    , @arg = 1
GO
