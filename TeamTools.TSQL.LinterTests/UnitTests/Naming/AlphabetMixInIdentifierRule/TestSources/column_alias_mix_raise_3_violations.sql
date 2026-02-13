
select 1 as ыi              -- 1
from src as s

select *
from
(
    values (1), (2)
) as t (ыi)                 -- 2

;with cte (ыi)              -- 3
as
(
    select foo
    from bar
)
select * from cte
