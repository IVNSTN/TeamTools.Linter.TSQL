DECLARE @a INT, @b INT, @c INT

WHILE (@a + @b) = dbo.my_fn(@c) -- function result is unpredictable
BEGIN
    SELECT @a + @b
END
GO

DECLARE @a INT, @b INT, @c INT

WHILE (@a + @b) = @c
BEGIN
    SET @a += 1 -- one of vars is enough
END
GO

DECLARE @a INT

WHILE (@a + 1) < 2 OR 1=0
BEGIN
    EXEC dbo.my_proc
        @param = @a OUTPUT
END
GO

DECLARE @a INT

WHILE (@a + 1) < 2 OR 1=0
BEGIN
    SELECT @a += 1
END
GO
