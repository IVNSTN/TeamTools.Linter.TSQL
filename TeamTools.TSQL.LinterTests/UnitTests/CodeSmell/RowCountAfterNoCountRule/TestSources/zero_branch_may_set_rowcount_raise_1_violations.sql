DECLARE @a INT;

IF @a > 10
BEGIN
    PRINT 'asdf'
END
ELSE IF @a < 0
    SET @a = 1;


SELECT @@ROWCOUNT;
