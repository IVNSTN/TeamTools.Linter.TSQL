DECLARE @str VARCHAR(3) = dbo.fn()

SET @str = ISNULL(@str, '1234567890') -- 1 isnull arg (not SET)
SELECT @str = ISNULL(@str, '    ') -- 2 isnull arg (not SET)

SELECT @str = ISNULL(@str, '12' + '34') -- 3 isnull arg (not SET)
