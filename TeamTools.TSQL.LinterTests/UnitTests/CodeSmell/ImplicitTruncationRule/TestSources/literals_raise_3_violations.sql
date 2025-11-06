DECLARE @str VARCHAR(3) = '12345'

SET @str = 'asd' + '1'

SET @str = 'asd' + (SELECT '1')
