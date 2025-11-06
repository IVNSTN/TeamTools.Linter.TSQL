DECLARE @i INT

WHILE @i > 0
BEGIN
    SELECT 1

    PRINT @i
END
GO

DECLARE @a INT, @b INT, @c INT

WHILE (@a + (@b)) = (@c + 1)
BEGIN
    SELECT @a + @b
END
GO

DECLARE @a INT, @b INT, @c INT = 1

WHILE (((@a + @b) = 1)
    AND @c >= 1)
BEGIN
    SET @c = 1 + @a + @b
    PRINT @c
END
GO
