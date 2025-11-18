exec sp_executesql
    'select cmd',
    '@date DATE, @sum MONEY OUTPUT, @name CHAR(1)',
    @badName = 'test',
    @missing_name = @sum OUTPUT,
    @date = @from_date
