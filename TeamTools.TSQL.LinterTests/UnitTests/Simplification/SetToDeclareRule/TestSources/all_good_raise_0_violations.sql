DECLARE @a INT = 2, -- already assigned
    @b DATETIME,
    @c CHAR(1)


IF EXISTS(SELECT 1 FROM dbo.foo)
BEGIN
    SELECT @b = GETDATE() -- under condition
END

SET @a = 3


EXEC dbo.bar
    @cc = @c OUTPUT -- assigned here

PRINT @c

SET @c = 'a'
