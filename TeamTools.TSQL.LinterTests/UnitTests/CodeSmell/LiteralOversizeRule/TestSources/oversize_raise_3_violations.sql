DECLARE @str VARCHAR(10)

SET @str = 'asdf' + '1234567890' -- 1

SET @str = ISNULL(@str, '12345678901') -- 2 in ISNULL
GO

DECLARE @name VARCHAR(5) = 'ABCDEFG' -- 3
