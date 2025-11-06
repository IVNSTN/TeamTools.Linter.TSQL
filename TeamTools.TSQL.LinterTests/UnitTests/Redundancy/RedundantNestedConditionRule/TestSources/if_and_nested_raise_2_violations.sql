IF @a > 0
    IF @a > 0 -- 1
        SET @a = 0

IF @a > 0
BEGIN
    IF @a =
        CASE
            WHEN ((@a > 0)) THEN 1 -- 2
            ELSE 0
        END
    BEGIN
        PRINT '1'
    end
END
