DECLARE @str VARCHAR(10) = 'AAA'

IF (SUBSTRING(@str, 1, 1) = 'A')
    PRINT 'OK'
