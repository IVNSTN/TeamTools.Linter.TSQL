DECLARE @str VARCHAR(10)

SET @str = 'asdf' + '1'

SET @str = ISNULL(@str, '1234567890')
SELECT ISNULL(@str, '')

PRINT GETDATE() -- zero args
