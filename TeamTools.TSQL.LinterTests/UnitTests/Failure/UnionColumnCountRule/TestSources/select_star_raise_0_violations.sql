select a, b, c, *
from t1

intersect

SELECT a, *
from t1

INTERSECT

select b, c, d, e
from t2

except

select *
from t3

union all

select *, a
from
(
    select d, e, f, g, h, i, j
    from t4

    union

    select f, *
    from t5
) t6
