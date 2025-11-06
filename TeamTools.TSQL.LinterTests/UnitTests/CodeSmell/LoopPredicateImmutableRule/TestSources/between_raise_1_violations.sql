DECLARE @a INT, @b INT, @c INT = 1

WHILE @a BETWEEN 1 AND @b
BEGIN
    SET @c = 1 + @a + @b
    PRINT @c
END
GO
