SELECT
    CASE @foo
        WHEN 'a' THEN 1
        WHEN 'b' THEN 2
        ELSE NULL
    END,
    CASE
        WHEN @bar = 0 THEN 'a'
        WHEN @bar = 1 THEN 'b'
        ELSE ((SELECT (NULL))) -- still detected
    END
