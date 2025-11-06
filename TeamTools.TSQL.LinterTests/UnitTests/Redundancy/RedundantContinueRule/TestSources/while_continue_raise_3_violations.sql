WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT @a

    FETCH NEXT FROM cr INTO @a

    CONTINUE    -- 1
END
GO

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT @a

    FETCH NEXT FROM cr INTO @a

    IF @a > 10
    BEGIN
        CONTINUE -- 2. This is still redundant
    END;
END
GO

WHILE @a < 10
    CONTINUE -- 3
GO
