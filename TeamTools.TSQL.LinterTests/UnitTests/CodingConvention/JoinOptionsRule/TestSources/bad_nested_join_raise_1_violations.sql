    SELECT top (100)
        t.*
    FROM (
        select *
        from (select *
        from a, b
        where z=d)t
    ) tt
