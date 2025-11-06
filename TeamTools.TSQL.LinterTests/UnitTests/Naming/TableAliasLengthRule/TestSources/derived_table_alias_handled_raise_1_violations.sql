select *
from (
    select ex.*
    from extracted ex
    cross join
    (
        select bb.b from bbb as bb
    ) AS d
) as tmp
