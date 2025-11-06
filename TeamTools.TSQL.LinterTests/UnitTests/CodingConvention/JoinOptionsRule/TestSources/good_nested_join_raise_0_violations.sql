    SELECT top (100)
        t.*
    FROM (
        select *
        from (select *
        from a cross join b
        where z=d)t
    ) tt
