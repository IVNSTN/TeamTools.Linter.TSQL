            EXEC sp_executesql
@stmt = N'select ''cmd'''
, @params = N'@date DATE, @sum MONEY OUTPUT, @name CHAR(1)'
, @name = @nm OUTPUT
, @sum = @sum
, @date = @from_date OUTPUT;
