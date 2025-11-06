SELECT COUNT(1)
WHERE EXISTS
    (
        SELECT
        1
        FROM
        dbo.bar
    )
