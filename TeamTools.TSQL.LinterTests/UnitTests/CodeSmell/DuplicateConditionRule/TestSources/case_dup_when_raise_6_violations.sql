SET @a =
    CASE @b
        WHEN '1' THEN 1
        WHEN (SELECT '1') THEN 100 -- 1
        WHEN @c THEN 2
        WHEN (@c) THEN 200 -- 2
        WHEN (@x * 2 - ABS(@z/100)) THEN 4
        WHEN (@x * 2 - ABS(@z/100)) THEN 400 -- 3
    END
GO

SET @a =
    CASE
        WHEN @b = '1' THEN 1
        WHEN @b = (SELECT '1') THEN 100 -- 4
        WHEN @c = 700 THEN 2
        WHEN (@c) = 700 THEN 200 -- 5
        WHEN (DATEADD(DD, 1, @dt) > GETDATE() AND @x * 2 < ABS(@z/100)) THEN 4
        WHEN (DATEADD(DD, 1, @dt) > GETDATE() AND @x * 2 < ABS(@z/100)) THEN 400 -- 6
    END
GO
