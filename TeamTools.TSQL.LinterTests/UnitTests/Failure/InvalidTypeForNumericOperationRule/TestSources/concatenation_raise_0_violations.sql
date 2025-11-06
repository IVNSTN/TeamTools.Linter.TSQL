DECLARE @str VARCHAR(10) = '1900', @dt DATETIME

SET @dt = CONVERT(DATETIME, @str + '.01.01') + 1

SET @str += 'asdf'
