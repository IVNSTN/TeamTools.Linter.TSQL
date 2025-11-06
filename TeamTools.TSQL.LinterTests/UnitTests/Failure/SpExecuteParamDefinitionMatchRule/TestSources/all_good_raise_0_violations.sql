-- ignored because @declaration
exec sp_executesql @cmd, @declaration, @var

-- ignored because no declaration
exec sp_executesql 'cmd'

-- ignored because declaration is empty
exec sp_executesql @params = '', @script = 'select cmd'

-- fine
exec sp_executesql
    @stmt = N'select cmd',
    @params = N'@date DATE, @sum MONEY NULL OUTPUT, @name CHAR(1)',
    @name = 'test', @sum = @sum OUTPUT, @date = @from_date

EXEC sys.sp_executesql
    @stmt = @sql_create
    , @params = N'@package_info VARCHAR(MAX)'
    , @package_info = @event_info;
