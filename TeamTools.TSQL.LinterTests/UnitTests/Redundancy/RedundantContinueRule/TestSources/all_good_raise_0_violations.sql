WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT @a

    IF @a > 10
        BREAK

    FETCH NEXT FROM cr INTO @a
END
GO

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT @a

    IF @a > 10
        CONTINUE

    FETCH NEXT FROM cr INTO @a
END
GO

WHILE @a < 10
    SET @a += 1
GO
