
SET @a =
    CASE
        WHEN ((@a > 0)) THEN
            CASE WHEN @b = 3 AND @a > 0 THEN 0 END -- 1
        ELSE
            CASE WHEN @a < 100 THEN
                CASE WHEN @a < 100 THEN 0 END -- 2
            END
    END
