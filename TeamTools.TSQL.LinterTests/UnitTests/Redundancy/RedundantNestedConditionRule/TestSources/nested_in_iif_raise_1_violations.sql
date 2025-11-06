-- compatibility level min: 110
SET @a =
    CASE
        WHEN ((@a > 0)) THEN
            IIF (@a > 0, -- here
                0, 1)
    END
