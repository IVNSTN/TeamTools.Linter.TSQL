SELECT x
FROM y
CROSS APPLY
            (
    SELECT z
    FROM f
    LEFT JOIN
        (
        SELECT z
        FROM f
        WHERE n = m
    ) j
    ON j.f_id = f.id
    WHERE n = m
) j
