IF @A IS NULL
    SELECT ISNULL(NULL, 1) + 3
    FROM c
    WHERE d IS NOT NULL

WHILE (NOT @e is null)
BEGIN
    SELECT
        CASE ISNULL(@h, 1)
            WHEN 0
            THEN NULL
        END as [NULL]
END
