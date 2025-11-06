IF @a > 0
    IF @a < 100
        SET @a = 0

SELECT
    CASE
        WHEN @a > 0
        THEN CASE WHEN @a < 100 THEN 0 ELSE 1 END
        ELSE @a
    END
