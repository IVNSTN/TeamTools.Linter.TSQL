SELECT
    CASE @h
        WHEN NULL -- 1
        THEN 1
    END as [NULL],
    CASE
        WHEN @j = NULL -- 2
        THEN 1
    END as [NULL],
    CASE (NULL) -- 3
        WHEN 1
        THEN (SELECT NULL)
    END
