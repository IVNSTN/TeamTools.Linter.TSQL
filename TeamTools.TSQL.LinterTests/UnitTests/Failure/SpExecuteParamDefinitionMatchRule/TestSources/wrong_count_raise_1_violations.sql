exec sp_executesql
    'select cmd',
    '@date DATE, @sum MONEY OUTPUT, @name CHAR(1)',
    'test'
