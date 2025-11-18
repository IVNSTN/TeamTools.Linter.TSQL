-- compatibility level min: 110
SELECT
    CASE @a
        WHEN 1 THEN
            IIF(@a = 1, '1', '2') -- here
        ELSE
            '3'
    END;
