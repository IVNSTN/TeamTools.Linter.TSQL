select *
from (select 1, a) as v(a, a)

select *
from (values (1), (2)) as v(b, b)
