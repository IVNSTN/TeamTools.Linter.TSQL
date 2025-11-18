select a, b, c
from t1

intersect
select a, b, c
from t1
intersect

select b, c, d, e
from t2

except

select b, c
from t3

union all

select a
from
(
    select d, e, f, g, h, i, j
    from t4

    union

    select f, g, h, i, j
    from t5
) t6
