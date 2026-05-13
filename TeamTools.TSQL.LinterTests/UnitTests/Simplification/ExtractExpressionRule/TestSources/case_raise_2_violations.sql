SELECT
    CASE
        WHEN a > b THEN 1
        WHEN b < 100 THEN 2
        ELSE 3
    END
FROM foo
WHERE
    CASE
        WHEN a > b THEN 1
        WHEN b < 100 THEN 2
        ELSE 3
    END > 0

SELECT
    CASE a
        WHEN 'a' THEN 1
        WHEN 'b' THEN 2
        ELSE 3
    END
FROM foo
WHERE
    CASE a
        WHEN 'a' THEN 1
        WHEN 'b' THEN 2
        ELSE 3
    END IS NOT NULL
