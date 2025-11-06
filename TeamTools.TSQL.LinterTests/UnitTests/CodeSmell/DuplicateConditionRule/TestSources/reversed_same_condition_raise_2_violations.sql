IF @a > @b
    PRINT 1
ELSE IF (SELECT (@b)) < @a -- 1
    PRINT 2;
GO

SELECT
    CASE
        WHEN (1 + ABS(@m) / 3) >= 0 THEN 1
        WHEN (0) <= (1 + ABS(@m) / 3) THEN 2 -- 2
    END
GO
