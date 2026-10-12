select null as a, 'b'
union all
select *
from (values (1)) a (b)
order by b


;with cte (a, b, c) as
(
select 'a', NULL, GETDATE()
)
select * from cte
order by a, b, c
