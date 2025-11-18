select *
from
(
    select 1, null, GETDATE()
) as src (id, name, dt, summary)
inner join
(
    select 1, null, GETDATE()
    union
    select 1, null, GETDATE()
) as dst (id, dt2)
on dst.id = src.id
