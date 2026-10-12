DECLARE @a INT;

IF @a > 10
BEGIN
    INSERT tbl
    EXEC my_proc
END
ELSE IF @a < 0
    SET @a = 1;


PRINT @@ROWCOUNT;
