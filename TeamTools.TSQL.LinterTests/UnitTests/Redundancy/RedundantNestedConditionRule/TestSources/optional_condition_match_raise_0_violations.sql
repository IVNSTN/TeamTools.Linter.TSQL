IF @a > 0 OR @b = 1
BEGIN
    IF @b = 1       -- this is not a dup
        PRINT 1
END

IF @a = CASE WHEN @b > 3 THEN 0 WHEN @b = 2 THEN 1 END
BEGIN
    IF @b = 2       -- this is not a dup
        PRINT 2
END
